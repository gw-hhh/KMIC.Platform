-- 迁移里的授权语句（GRANT ... TO kmic_runtime）要求这个角色已经存在；生产环境由部署流程创建，开发容器在这里创建
CREATE ROLE kmic_runtime NOLOGIN;
