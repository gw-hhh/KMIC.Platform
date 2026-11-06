-- 原型 #2 第 3 节：数据范围过滤器的 SQL 形状在 PG 18 上的执行计划
-- 运行：docker run -d --rm --name kmic-plan -e POSTGRES_PASSWORD=x postgres:18
--       docker exec -i kmic-plan psql -U postgres < data-scope-plans.sql
-- 20 万行、200 个组织单元、2000 个用户、10% 私有；当前用户 ...0007

CREATE TABLE s (id uuid PRIMARY KEY DEFAULT uuidv7(), owner uuid NOT NULL, org uuid NOT NULL, is_private bool NOT NULL, is_deleted bool NOT NULL DEFAULT false);
CREATE TABLE c (sid uuid NOT NULL, uid uuid NOT NULL, PRIMARY KEY (sid, uid));
CREATE INDEX ON s (owner);
CREATE INDEX ON s (org);
CREATE INDEX ON c (uid, sid);

INSERT INTO s (owner, org, is_private)
SELECT ('00000000-0000-0000-0000-' || lpad((g % 2000)::text, 12, '0'))::uuid,
       ('00000000-0000-0000-0001-' || lpad((g % 200)::text, 12, '0'))::uuid,
       g % 10 = 0
FROM generate_series(1, 200000) g;
INSERT INTO c SELECT id, '00000000-0000-0000-0000-000000000007'::uuid FROM s TABLESAMPLE SYSTEM (0.1);
ANALYZE;

SET max_parallel_workers_per_gather = 0;

-- 只有所属人、组织两个分支
PREPARE a(bool, uuid, bool, uuid, uuid[]) AS
SELECT s.id FROM s
WHERE NOT s.is_deleted AND ($1 OR ((NOT s.is_private OR s.owner = $2) AND ($3 OR s.owner = $4 OR s.org = ANY ($5))));

\echo ==== 只有所属人、组织：按实际参数值做计划
SET plan_cache_mode = force_custom_plan;
EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY ON)
EXECUTE a(false, '00000000-0000-0000-0000-000000000007', false, '00000000-0000-0000-0000-000000000007', ARRAY['00000000-0000-0000-0001-000000000003'::uuid]);

\echo ==== 只有所属人、组织：通用计划
SET plan_cache_mode = force_generic_plan;
EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY ON)
EXECUTE a(false, '00000000-0000-0000-0000-000000000007', false, '00000000-0000-0000-0000-000000000007', ARRAY['00000000-0000-0000-0001-000000000003'::uuid]);

SET plan_cache_mode = force_custom_plan;

-- 加上关系可见（EF 生成的 EXISTS）
PREPARE b(bool, uuid, bool, uuid, uuid[], uuid) AS
SELECT s.id FROM s
WHERE NOT s.is_deleted AND ($1 OR ((NOT s.is_private OR s.owner = $2) AND ($3 OR s.owner = $4 OR s.org = ANY ($5)
      OR EXISTS (SELECT 1 FROM c WHERE c.sid = s.id AND c.uid = $6))))
ORDER BY s.id LIMIT 20;

PREPARE d(bool, uuid, bool, uuid, uuid[], uuid) AS
SELECT count(*) FROM s
WHERE NOT s.is_deleted AND ($1 OR ((NOT s.is_private OR s.owner = $2) AND ($3 OR s.owner = $4 OR s.org = ANY ($5)
      OR EXISTS (SELECT 1 FROM c WHERE c.sid = s.id AND c.uid = $6))));

-- 关系条件改写为 id = ANY (ARRAY(子查询))
PREPARE e(bool, uuid, bool, uuid, uuid[], uuid) AS
SELECT count(*) FROM s
WHERE NOT s.is_deleted AND ($1 OR ((NOT s.is_private OR s.owner = $2) AND ($3 OR s.owner = $4 OR s.org = ANY ($5)
      OR s.id = ANY (ARRAY(SELECT c.sid FROM c WHERE c.uid = $6)))));

\echo ==== 关系可见 EXISTS：列表页，只看本人
EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY ON)
EXECUTE b(false, '00000000-0000-0000-0000-000000000007', false, '00000000-0000-0000-0000-000000000007', ARRAY[]::uuid[], '00000000-0000-0000-0000-000000000007');

\echo ==== 关系可见 EXISTS：计数，只看本人
EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY ON)
EXECUTE d(false, '00000000-0000-0000-0000-000000000007', false, '00000000-0000-0000-0000-000000000007', ARRAY[]::uuid[], '00000000-0000-0000-0000-000000000007');

\echo ==== 关系可见改写为 = ANY (ARRAY(...))：计数，只看本人
EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY ON)
EXECUTE e(false, '00000000-0000-0000-0000-000000000007', false, '00000000-0000-0000-0000-000000000007', ARRAY[]::uuid[], '00000000-0000-0000-0000-000000000007');
