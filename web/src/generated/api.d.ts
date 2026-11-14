// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改

export interface paths {
    "/api/v1/notifications/captured/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一条捕获的通知内容
         * @description 包含正文。没有开启捕获（生产环境）时返回 404。内容可能含验证码，查看会记读审计。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 捕获内容 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CapturedDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/captured": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询捕获的通知内容
         * @description 非生产环境不真的发出邮件和短信，内容留在这里供测试人员查看，不含正文。没有开启捕获（生产环境）时返回 404。内容可能含验证码，查看会记读审计。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 按渠道筛选：Email、Sms */
                    Channel?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -capturedAt：字段名前加 - 表示倒序，默认新的在前 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfCapturedListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/deliveries/{id}/abandon": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 放弃一条失败的投递
         * @description 状态仍是失败，加上放弃标记，不再出现在待处理清单里。只有失败、而且还没放弃的投递可以放弃，否则返回 422。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 投递 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["AbandonDeliveryRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/deliveries/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一条投递
         * @description 包含渲染后的主题和正文。含密模板的投递发完或最终失败后，正文已经清空。查看会记读审计。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 投递 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DeliveryDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/deliveries": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询投递记录
         * @description 每条是把一条通知用一个渠道发给一个收件人：状态、脱敏地址、尝试次数、失败原因，不含正文。可以按收件用户、渠道、状态、模板、创建时间筛选，分页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 按收件用户筛选 */
                    RecipientUserId?: string;
                    /** @description 按渠道筛选：Email、Sms */
                    Channel?: string;
                    /** @description 按状态筛选：Pending、Sending、Sent、Failed、Skipped、Captured */
                    Status?: string;
                    /** @description 按模板键筛选 */
                    TemplateKey?: string;
                    /** @description 创建时间起，含 */
                    From?: string;
                    /** @description 创建时间止，不含 */
                    To?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -createdAt：逗号分隔，字段名前加 - 表示倒序，默认新的在前 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfDeliveryListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/deliveries/{id}/retry": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 重试一条失败的投递
         * @description 放回队列，尝试次数清零。只有失败、而且没有放弃的投递可以重试，否则返回 422；含密通知超过有效期的不能重试。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 投递 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/site-messages/unread-count": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询我的未读站内消息数
         * @description 前端每 30 秒轮询一次，响应很小。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["UnreadCountResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/site-messages": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询我的站内消息
         * @description 只有自己的消息。默认未读在前、新的在前，可以只看未读或已读、按类别筛选，分页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 只看未读（true）或只看已读（false），不给就都看 */
                    Unread?: boolean;
                    /** @description 只看某个类别 */
                    Category?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 isRead,-sentAt：逗号分隔，字段名前加 - 表示倒序，默认未读在前、新的在前 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfSiteMessageItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/site-messages/read-all": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 把我的全部未读站内消息标为已读
         * @description 返回这次标了几条。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MarkedReadResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/site-messages/{id}/read": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 把一条站内消息标为已读
         * @description 已经读过的再标一次什么也不改。别人的消息和不存在的一样，返回 404。成功返回 204。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 消息 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/templates/{key}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一个通知模板
         * @description 各渠道当前的内容和代码里的默认内容、模板里能用的变量、示例数据、发布过的版本。响应里的 version 在发布和恢复时作为期望版本传回。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 模板键，如 Sample.SpecimenSubmitted */
                    key: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["TemplateDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/templates": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询全部通知模板
         * @description 每个模板的渠道、状态、各渠道生效的版本号和最近一次发布的时间。没有发布过版本的渠道用代码里的默认内容。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["TemplateListResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/templates/{key}/preview": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 预览模板内容的渲染结果
         * @description 不保存。渲染的限时和输出上限与正式发送一样。模板有误时仍返回 200，原因放在 error 里，方便边写边看。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 模板键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["PreviewTemplateRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["TemplatePreviewResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/templates/{key}/publish": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 给模板的一个渠道发布新版本
         * @description 发布后立即生效。发布前用示例数据渲染一遍，语法错误、超时、输出过大返回 422。已退役的模板不能发布。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 模板键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["PublishTemplateRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfNotificationTemplateId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/notifications/templates/{key}/revert": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 恢复模板某个渠道的默认内容
         * @description 这个渠道改回用代码里的内容，发布过的历史版本保留。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 模板键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RevertTemplateRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfNotificationTemplateId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询文件详情
         * @description 元数据、状态、引用和彻底删除的记录。不返回存储键和下载地址：管理员也不经这里下载内容，下载走业务对象。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 文件 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["FileDetail"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询文件列表
         * @description 文件的元数据、状态和引用，不含内容和存储键，分页。可以按状态、上传档案、上传人、上传时间和引用它的业务对象筛选。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在文件名里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，重复参数名可以给多个，如 status=Available&amp;status=Attached */
                    Status?: string[];
                    /** @description 上传档案 */
                    Profile?: string;
                    /** @description 上传的人 */
                    UploadedBy?: string;
                    /** @description 上传时间不早于 */
                    From?: string;
                    /** @description 上传时间早于 */
                    To?: string;
                    /** @description 只看被这个模块的业务对象引用的文件 */
                    OwnerModule?: string;
                    /** @description 业务对象的类型，和 ownerModule 一起给 */
                    OwnerType?: string;
                    /** @description 业务对象的 ID，和 ownerModule、ownerType 一起给 */
                    OwnerId?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -uploadedAt,name：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfFileListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        /**
         * 上传一个文件
         * @description multipart/form-data，一次一个文件，第一个带文件名的部分就是文件。profile 是上传档案名，决定大小上限、允许的类型和需要的权限点；类型按扩展名加文件头识别，不信任客户端声明的内容类型。成功得到文件 ID：这是临时文件，到期（默认 24 小时）还没有被业务对象关联就会清理。文件太大返回 413，类型不允许返回 415。
         */
        post: {
            parameters: {
                query: {
                    /** @description 上传档案名，如 Sample.ContractScan */
                    profile: string;
                    /** @description 文件大小（字节），客户端知道时给，超过档案上限直接拒绝；不给就边读边按上限截断 */
                    size?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "multipart/form-data": components["schemas"]["IFormFile"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["UploadStarted"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/storage-issues": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询存储问题
         * @description 对账发现库里有记录而存储里找不到对象的文件，分页。“可以还原”的对象版本还在，运维按手册还原后用“标为已解决”关闭。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 状态编码，重复参数名可以给多个，如 status=Open */
                    Status?: string[];
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -foundAt */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfStorageIssueItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/{id}/purge": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 彻底删除文件
         * @description 内容无法再读取，不可恢复；引用它的单据显示“文件已删除”。原因必填。历史版本要运维另行清除。已经彻底删除的再来一次算成功。业务正在关联这个文件时返回 503，稍后再试。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 文件 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["PurgeFileRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfStoredFileId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/{id}/history-purge": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 登记已彻底删除历史版本
         * @description 运维在对象存储里清除了这个文件的历史版本之后，在这里留执行记录（如工单号）。只有已经彻底删除的文件可以登记。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 文件 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RecordHistoryPurgeRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfStoredFileId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/storage-issues/{id}/resolve": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 标记存储问题已解决
         * @description 运维把对象还原之后用。处理说明必填。已经解决的再来一次算成功。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 存储问题 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ResolveStorageIssueRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfStorageIssueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/files/{id}/retry-deletion": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 重试删除
         * @description 删除失败太多次、不再自动重试的文件，排除存储的问题之后用它清零尝试次数。文件不在删除队列里返回 404。成功返回 204。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 文件 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/audit/exports/{jobId}/file": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 下载审计导出的文件
         * @description 只有提交导出的人和有管理作业权限的人能下载，别人看到的和不存在的一样，返回 404。作业没做完、文件还没登记完返回可重试的 409，作业失败或被取消返回 422。每次下载都有审计记录。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 导出作业 ID */
                    jobId: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["FileDownload"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/audit/exports": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 导出审计记录
         * @description 把一段时间的审计记录导成 CSV 文件，筛选条件和审计列表一样。做成后台作业：返回 202，响应里的 jobId 是作业 ID，Location 指向结果文件，作业没做完时返回 409，稍后再取。行数超过上限时作业失败，要缩小时间范围。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ExportAuditRequest"];
                };
            };
            responses: {
                /** @description 已受理，Location 指向后台作业，响应体是作业 ID */
                202: {
                    headers: {
                        /** @description 后台作业的地址 */
                        Location?: string;
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["JobAccepted"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/audit/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一条审计记录的详情
         * @description 含字段级变更与补充说明；操作人、角色的名称实时解析，已经不存在的显示为空。这个查询本身也会留下读取审计。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 审计记录 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["AuditDetail"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/audit": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询审计记录
         * @description 时间范围必填，最长 93 天；按发生时间倒序，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。不套数据范围，审计人员看到全部；这个查询本身也会留下读取审计。
         */
        get: {
            parameters: {
                query: {
                    /** @description 起始时间，含；必填 */
                    From: string;
                    /** @description 结束时间，不含；必填，和起始时间相隔不超过 93 天 */
                    To: string;
                    /** @description 操作人 */
                    ActorUserId?: string;
                    /** @description 实际操作人：模拟登录时真正动手的人 */
                    ActualUserId?: string;
                    /** @description 产生审计的模块，如 Identity */
                    Module?: string;
                    /** @description 操作名，如 Identity.CreateUser */
                    Action?: string;
                    /** @description 操作对象的类型，如 User */
                    ResourceType?: string;
                    /** @description 操作对象的 ID */
                    ResourceId?: string;
                    /** @description 上一页返回的游标；第一页不给 */
                    After?: string;
                    /** @description 每页数量，1 到 200，默认 50 */
                    Limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CursorPageOfAuditListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/background-jobs/{module}/{id}/cancel": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 请求取消一个后台作业
         * @description 排队中的直接取消，运行中的改为“取消中”，执行器在下次续期时停下。取消不是回滚，已提交的批次保留。作业已经结束返回 409 Operations.BackgroundJobFinished。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 作业所在的模块，如 Sample */
                    module: string;
                    /** @description 作业 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/my-jobs/{module}/{id}/cancel": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 取消我提交的一个后台作业
         * @description 排队中的直接取消，运行中的改为“取消中”，执行器在下次续期时停下。取消不是回滚，已提交的批次保留。别人提交的作业和不存在的一样返回 404；作业已经结束返回 409 Operations.BackgroundJobFinished。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 作业所在的模块，如 Sample */
                    module: string;
                    /** @description 作业 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/my-jobs/{module}/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询我提交的一个后台作业
         * @description 页面轮询进度用，登录就能看，建议每 2 秒查一次、结束后停止。别人提交的作业和不存在的一样返回 404 Operations.BackgroundJobNotFound。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 作业所在的模块，如 Sample */
                    module: string;
                    /** @description 作业 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["BackgroundJobItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/background-jobs": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询各模块的后台作业
         * @description 遍历每个模块自己的作业表，按提交时间倒序归并，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。能看到状态、进度、结果摘要和错误，看不到作业参数。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 只看这个模块的；不给就是全部模块 */
                    Module?: string;
                    /** @description 作业类型名，如 Sample.ExportSpecimens */
                    JobType?: string;
                    /** @description 状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled */
                    Status?: string;
                    /** @description 提交人 */
                    SubmittedBy?: string;
                    /** @description 上一页返回的游标；第一页不给 */
                    After?: string;
                    /** @description 每页数量，1 到 200，默认 50 */
                    Limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CursorPageOfBackgroundJobItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/my-jobs": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询我在各模块提交的后台作业
         * @description 登录就能看，只看得到自己提交的；外壳的“作业入口”用它。按提交时间倒序，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled */
                    Status?: string;
                    /** @description 上一页返回的游标；第一页不给 */
                    After?: string;
                    /** @description 每页数量，1 到 200，默认 50 */
                    Limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CursorPageOfBackgroundJobItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/client-errors": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 前端批量上报错误
         * @description 登录就能报（包括必须先改密码的账号），只写日志（分类 Kmic.ClientErrors），不落库，也不要求幂等键。一次最多 20 条，每个用户每分钟最多 30 次请求，超限返回 429，前端丢弃这批、不重试。字段是白名单，不接收请求体、表单值和地址里的查询参数。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ReportClientErrorsRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/abandon": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 放弃收件箱里的死信，单条或批量
         * @description 放弃意味着这些消息永远不会被处理，原因必填。放弃之前先调接收方为这种消息注册的放弃检查，不让放弃的不改状态，回执里写明原因，只能重试。只有收件箱的死信能放弃。一次最多 200 条。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["AbandonMessagesRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MessageReceipts"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/rebuilds/abandon": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 放弃读模型重建
         * @description 放弃暂停的一轮，并清除订阅的需要初始化标记：此后不会自动重建，要重建由运维手动触发。没有暂停的一轮返回 409。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RebuildRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/rebuilds/confirm": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 确认读模型重建收尾
         * @description 源端找不到的行太多而暂停的一轮，运维核实它们确实没有了之后确认：另排一个作业把这些行置为终态并完成这一轮。没有暂停的一轮返回 409。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RebuildRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/routes/finalize": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 终结订阅
         * @description 只有待终结的订阅能终结，之后不再向它投递，旧构建不能自动恢复它，要在受控的维护窗口里做。还有运行中的实例在声明它、有消息正在处理时拒绝。有没处理完的消息时，选择放弃（原因连同原来的错误一起记下）才能终结。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["FinalizeRouteRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["FinalizeRouteResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/dead/{module}/{box}/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查看一条死信的全部内容
         * @description 包括载荷与信封，也能看已经被放弃的消息。载荷原样显示：死信载荷按设计不含敏感字段，含密消息的载荷不进库。每次查看都会留下读取审计。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 所在模块，如 Sample */
                    module: string;
                    /** @description Outbox 或 Inbox */
                    box: string;
                    /** @description 行 ID，来自死信列表 */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DeadMessageDetailItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/summary": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 各模块的消息积压汇总
         * @description 每个模块的发件箱积压、最老一条的年龄、死信数，以及收件箱里每个订阅的积压、死信、被挡住的顺序键。待终结订阅的积压是预期内的，带 isRetiring 标记，不要和普通订阅的积压一起告警。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MessageSummaryResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/blocked-keys": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 被有序订阅里的死信挡住的顺序键
         * @description 同一个键下排在死信后面的消息都在等它：处理掉最早的那条死信（重试，或确认不需要而放弃）才能放开。每个模块最多 500 个键，等得最多的排前面。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 只看这个模块的；不给就是全部 */
                    module?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["BlockedKeyItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/dead": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询各模块的死信
         * @description 遍历每个模块自己的发件箱与收件箱，按发生时间倒序归并，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。只列死信，不含载荷；载荷用详情接口看。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 只看这个模块的；不给就是全部模块 */
                    Module?: string;
                    /** @description Outbox 或 Inbox */
                    Box?: string;
                    /** @description 订阅名；给了就只看收件箱 */
                    Subscription?: string;
                    /** @description 消息名称 */
                    MessageType?: string;
                    /** @description 上一页返回的游标；第一页不给 */
                    After?: string;
                    /** @description 每页数量，1 到 200，默认 50 */
                    Limit?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CursorPageOfDeadMessageItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/instances": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询运行中的实例
         * @description 每个实例的构建号、角色、最近一次心跳和它声明的订阅，最近有心跳的排前面。只用于诊断：判断哪些构建还在声明某个订阅。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["InstanceItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/rebuilds": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询读模型重建
         * @description 每个订阅最近几轮重建，新的在前。状态为等待确认的一轮，源端找不到的行太多而暂停了，要运维核实后确认收尾或放弃。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["RebuildItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/routes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询订阅路由
         * @description 哪种消息投递给哪个模块的哪个订阅，状态（生效、待终结、已终结），以及现在还在运行的实例里哪些构建声明了它。终结一个订阅之前先看这里。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 状态编码，重复参数名可以给多个，如 status=Retiring；不给就是全部 */
                    status?: string[];
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["RouteItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/routes/reactivate": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 重新激活订阅
         * @description 只有已终结的订阅能重新激活，恢复成生效，从现在起重新投递。终结期间漏掉的消息不会自动补回，要核对缺口区间再补投。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ReactivateRouteRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/redeliver": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 补投
         * @description 把来源模块发件箱里已经投递过的、序号落在区间里的消息，再投递给指定订阅；订阅收到过的不会重复写入。一次最多 5000 条，超过要缩小区间。订阅已终结时要先重新激活。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RedeliverRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["RedeliverResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/retry": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 重试死信，单条或批量
         * @description 状态回到待处理，次数清零，马上可以被领取；一次最多 200 条。每条各有各的回执，已经不是死信的、不存在的不影响别的。重试不保证成功：问题没解决的消息会再次失败。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RetryMessagesRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MessageReceipts"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/messages/rebuilds/trigger": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 手动触发读模型重建
         * @description 给订阅手动排一轮重建，订阅要声明了重建方式。已经有排队或进行中、暂停等确认的一轮时返回 409。成功返回 204。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RebuildRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs/{name}/runs": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一个定时任务的执行记录
         * @description 按开始时间倒序，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。已经从代码里删除的任务，留下的记录也能查。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 上一页返回的游标；第一页不给 */
                    After?: string;
                    /** @description 每页数量，1 到 200，默认 50 */
                    Limit?: number;
                };
                header?: never;
                path: {
                    /** @description 任务名，如 Identity.PurgeSessions */
                    name: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CursorPageOfScheduledJobRunItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询全部定时任务
         * @description 模块的和框架自带的都列出来，每项带代码里的默认值、运维改过的覆盖值、调度器里实际生效的状态（下次触发时间、是否已同步）、最近一次执行和当前用户可用的动作。代码里已经删除而覆盖设置还留着的任务也在，标为已删除。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["ScheduledJobItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs/cron-preview": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 检查 Cron 表达式并预览接下来 5 次触发时间
         * @description 按业务时区解释，写法是 Quartz 的（带秒，如 0 0 7 * * ?）。写法不对、以后不会再触发、两次执行间隔短于 1 分钟，都返回 cron 字段的错误。
         */
        get: {
            parameters: {
                query: {
                    /** @description Cron 表达式 */
                    cron: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CronPreviewResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs/{name}/restore-default": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 把一个定时任务恢复成代码里的默认开关与执行时间
         * @description 设置行保留、两项清空，便于追溯，之后跟着代码里的默认值变化。一直没改过的任务没有可恢复的，返回 404。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 任务名 */
                    name: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RestoreJobDefaultRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfScheduledJobSettingId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs/{name}/run": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 立即执行一次定时任务
         * @description 交给运行着调度器的实例执行，几秒内开始；执行记录里触发方式是“手动”、触发人是操作人，结果到执行记录里看。任务正在运行时返回 409 Operations.JobAlreadyRunning，不排队。停用的任务也可以手动执行。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 任务名 */
                    name: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/operations/scheduled-jobs/{name}/override": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 改一个定时任务的开关和执行时间
         * @description 整体替换覆盖设置：给空的项恢复成跟着代码里的默认值。保存后同步到调度器，整个集群即时生效；同步失败不影响保存，列表里 inSync 会是 false。必须带幂等键。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 任务名 */
                    name: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetJobOverrideRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfScheduledJobSettingId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 添加字典项
         * @description 编码在同一类型内唯一（不区分大小写），创建后不能改。不分层的字典不能给父项。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["AddDictionaryItemRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items/{code}/disable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 停用字典项
         * @description 历史数据仍然显示“已停用”，新数据不能再选。下面有启用的子项时返回 422，先停用子项。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                    /** @description 字典项编码 */
                    code: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["DictionaryItemVersionRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items/{code}/enable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 启用字典项
         * @description 它的父项必须是启用的。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                    /** @description 字典项编码 */
                    code: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["DictionaryItemVersionRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionaries": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 取一批字典
         * @description types 是逗号分隔的字典类型编码，一次最多 20 个。登录即可，含停用的项并带状态；不存在的类型不出现在结果里。响应带 ETag（各类型版本的组合），前端带 If-None-Match 再问，没变就返回 304。
         */
        get: {
            parameters: {
                query: {
                    /** @description 字典类型编码，逗号分隔，一次最多 20 个 */
                    types: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DictionariesResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询一个字典类型的全部字典项
         * @description 含停用的项与版本号，管理页使用；修改字典项时带上这里的 version。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 字典类型编码，如 Sample.Industry */
                    typeCode: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DictionaryTypeDetail"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询全部字典类型
         * @description 类型、所属模块、字典项数、状态。字典类型由代码声明，这里只能维护它们的字典项。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DictionaryTypeListItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items/{code}/move": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 移动字典项
         * @description 只对层级字典；排在新父项下的最后。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                    /** @description 字典项编码 */
                    code: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["MoveDictionaryItemRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items/{code}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 修改字典项的显示名
         * @description 只改显示名，编码不能改：业务数据里存的是编码。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                    /** @description 字典项编码 */
                    code: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RenameDictionaryItemRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/dictionary-types/{typeCode}/items-order": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 调整同一父项下全部字典项的次序
         * @description 整体提交：必须包含该父项下的全部字典项，缺项或多项都拒绝，避免别人同时添加的项被丢掉。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 字典类型编码 */
                    typeCode: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ReorderDictionaryItemsRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfDictionaryTypeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/features": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询当前开放的功能开关
         * @description 登录就能看。返回开启的开关名，前端启动时拉一次，据此显示或隐藏入口；开关只决定功能是否开放，不授予权限。开关的值来自部署配置，改了要重启生效。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["FeaturesResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询本人的偏好
         * @description 当前用户在当前角色下全部偏好的生效值，每项带着值来自哪一层（默认、系统、角色、本人）。包括偏好族里已经有值的键。前端启动时拉一次，修改后更新本地。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MyPreferencesResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询偏好清单
         * @description 管理页用：目录里声明的偏好和偏好族里已经有系统层或角色层值的键，各自允许的层、默认值、系统层和各角色层现有的值与版本。本人那一层不在这里。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PreferenceDefinitionsResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/me/{key}/reset": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 恢复本人的一个偏好
         * @description 删除本人这一层，值回落到角色层，再回落到系统层和默认值。本来就没有设置过时什么也不做。模拟登录时不能操作。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 偏好键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/{key}/layers/roles/{roleId}/reset": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 删除偏好在一个角色上的值
         * @description 只删这个角色这一层，系统层的值或默认值重新对这个角色下没有自己设置过的人生效。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 偏好键 */
                    key: string;
                    /** @description 角色 ID */
                    roleId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ResetPreferenceLayerRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfPreferenceValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/{key}/layers/system/reset": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 删除偏好的系统层
         * @description 只删系统层，代码里的默认值重新对没有被覆盖的人生效。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 偏好键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ResetPreferenceLayerRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfPreferenceValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/me/{key}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 保存本人的一个偏好
         * @description 只存本人在当前角色下的这一份，换了角色是另一份。偏好族里的键只校验格式、必须是 JSON 对象且不超大小，内容由前端自己约定。模拟登录时不能保存。同一个键再写一次是覆盖。
         */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 偏好键，如 Ui.Shell 或 Ui.Table.Sample.Specimens.Main */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetMyPreferenceRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/{key}/layers/roles/{roleId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 设置偏好在一个角色上的值
         * @description 这个角色下没有自己设置过的人会看到它。角色必须存在。声明里不允许角色层的偏好会被拒绝。必须带幂等键。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 偏好键 */
                    key: string;
                    /** @description 角色 ID */
                    roleId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetPreferenceLayerRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfPreferenceValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/preferences/{key}/layers/system": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 设置偏好的系统层
         * @description 对所有人生效，没有被角色层或本人覆盖的人才会看到。声明里不允许系统层的偏好会被拒绝。必须带幂等键。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 偏好键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetPreferenceLayerRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfPreferenceValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/settings/{key}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** 查询一个系统设置 */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 设置键，如 Sample.Submission */
                    key: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["SettingItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        /**
         * 修改一个系统设置
         * @description 需要“查看设置”权限点，另外还要这个设置自己声明的修改权限点（列表里每项都带着当前用户能不能改）。值按设置的值类型读出后执行它自己的校验，通不过返回逐字段的错误。必须带幂等键。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 设置键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetSettingRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSystemSettingValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/setting-changes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询系统设置的变更历史
         * @description 谁在什么时候把什么改成了什么，为什么。新旧值原样保存，所以设置里不放密钥。默认按时间倒序，分页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 只看这个设置的；不给就是全部 */
                    Key?: string;
                    /** @description 起始时间，含 */
                    From?: string;
                    /** @description 结束时间，不含 */
                    To?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -changedAt,key：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfSettingChangeItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/settings": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询全部系统设置
         * @description 代码里声明的设置：值里的成员清单、默认值、当前值、是否被修改过、修改它需要的权限点，以及当前用户能不能改；另列代码里已经不存在、值行还留着的设置。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["SettingsResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/settings/{key}/purge": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 清除代码里已经不存在的设置留下的值
         * @description 仍然在代码里声明的设置不能这样清，要用恢复默认。需要敏感权限点“清除不再使用的设置值”。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 设置键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SettingReasonRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSystemSettingValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/configuration/settings/{key}/reset": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 把系统设置恢复成默认值
         * @description 删除被修改过的值，并记一条新值为空的变更。需要的权限点同修改。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 设置键 */
                    key: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SettingReasonRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSystemSettingValueId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/auth/change-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 修改自己的密码
         * @description 已经有密码时要给旧密码；还没有设置过密码的用户（靠短信登录的）不要求。成功后其他会话全部作废，当前会话继续。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ChangePasswordRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 当前用户
         * @description 当前用户、当前角色、可切换的角色、当前角色的权限点、授权版本、是否必须改密码、是否模拟身份和业务时区。当前角色不可用时也能调用，前端据此引导重新选择角色。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MeResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/me/sessions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询我的登录设备
         * @description 自己当前有效的会话：设备、IP、登录时间、最近刷新时间，标出哪个是当前会话。不含别人以我的身份开始的模拟会话。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["MySessionItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/auth/login": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 账号密码登录
         * @description 成功返回访问令牌；网页客户端的刷新令牌放在 HttpOnly Cookie 里，不出现在响应体，移动端在响应体里取得。账号不存在、密码错误、账号已禁用返回同样的 401；登录尝试过多返回 429。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["LoginRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["AuthTokens"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/auth/logout": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 退出登录
         * @description 作废当前会话并清除刷新令牌 Cookie。天然幂等。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/auth/refresh": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 刷新令牌
         * @description 网页客户端从 HttpOnly Cookie 取刷新令牌（要求可信 Origin 与 X-Requested-With: kmic），移动端在请求体里给出 refreshToken。每次刷新换一个新的刷新令牌；旧令牌宽限期之外再次出现，整个会话作废并返回 401。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json": null | components["schemas"]["RefreshRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["AuthTokens"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/me/sessions/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /**
         * 结束我的一个登录设备
         * @description 只能结束自己的会话，别人的会话返回 404。结束当前会话等于退出登录。天然幂等。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 会话 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/auth/switch-role": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 切换当前角色
         * @description 在同一会话里换成用户拥有的另一个启用的角色，返回带新角色的访问令牌；旧角色的访问令牌随即失效。当前角色被收回或停用后也能用它重新选择角色。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SwitchRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["AuthTokens"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/impersonation": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /**
         * 结束当前的模拟会话
         * @description 用模拟令牌调用；不是模拟会话时返回 422 Identity.NotImpersonating。只读的模拟会话里也能调用。前端同时丢弃模拟令牌。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/impersonation": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 开始模拟身份
         * @description 创建一个独立的模拟会话，管理员自己的会话不受影响。响应只有访问令牌，没有刷新令牌，有效期就是会话上限（默认 1 小时）；用它调用接口时，操作人是被模拟的用户，审计记录里另有实际操作人。默认只读，另有“模拟身份时允许写入”权限点时可以修改数据。不能模拟自己、系统账号和受保护角色的持有人。不需要幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 被模拟的用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["StartImpersonationRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["AuthTokens"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/org-unit": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 设置用户所属的组织单元
         * @description 直接改变用户的数据范围：“本组织”“本组织及下级”都从这里算起。调整之后他名下已有数据的所属组织不变。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["AssignUserOrgUnitRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 添加组织单元
         * @description 在某个启用的单元下添加下级，排在同级最后。响应里的 version 是添加之后的组织树版本号。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["CreateOrgUnitRequest"];
                };
            };
            responses: {
                /** @description 已创建，Location 指向新数据，响应体是新数据的 ID 与版本号 */
                201: {
                    headers: {
                        /** @description 新数据的地址 */
                        Location?: string;
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CreatedResponseOfOrgUnitId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units/{id}/disable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 停用组织单元
         * @description 还有启用的下级、或有启用的用户归属于它时返回 409 Identity.OrgUnitInUse；根单元不能停用。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 组织单元 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["OrgUnitVersionRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfOrgTreeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units/{id}/enable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 启用组织单元
         * @description 它的上级必须是启用的。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 组织单元 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["OrgUnitVersionRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfOrgTreeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-tree": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询整棵组织树
         * @description 登录即可用。平铺的全部单元（含已停用的），按上级分组、同一上级下按次序排好，前端按 parentId 拼成树；version 是修改组织树时要带的版本号。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["OrgTreeResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units/{id}/move": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 移动组织单元
         * @description 连同它的下级一起移到另一个单元之下，排在新上级的最后。不能移到自己或自己的下级之下，根单元不能移动。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 组织单元 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["MoveOrgUnitRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfOrgTreeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units/{parentId}/children-order": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 调整某个单元下全部直接下级的次序
         * @description 整体提交：必须包含该单元下的全部直接下级，且不重复。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 上级组织单元 ID */
                    parentId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ReorderOrgUnitsRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfOrgTreeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/org-units/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /** 修改组织单元的名称与编码 */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 组织单元 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["UpdateOrgUnitRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfOrgTreeId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询角色列表
         * @description 角色、状态、是否受保护和持有人数，分页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在名称和说明里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，重复参数名可以给多个，如 status=Active&amp;status=Disabled */
                    Status?: string[];
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 sortOrder,-holderCount：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfRoleListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        /**
         * 新建角色
         * @description 新建一个没有任何权限点的角色，之后用设置权限点与数据范围的接口配置。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["CreateRoleRequest"];
                };
            };
            responses: {
                /** @description 已创建，Location 指向新数据，响应体是新数据的 ID 与版本号 */
                201: {
                    headers: {
                        /** @description 新数据的地址 */
                        Location?: string;
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CreatedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles/{id}/disable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 停用角色
         * @description 持有人正在用它的会话，下一个请求起得到 403，要重新选择角色。内置的系统管理员角色不能停用。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["DisableRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles/{id}/enable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** 启用角色 */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["EnableRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询角色详情
         * @description 权限点（已退役的标出）、数据范围配置、持有人数、版本号和当前用户可以执行的动作。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["RoleDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        /**
         * 修改角色的名称、说明和次序
         * @description 受保护角色只有当前角色也是受保护角色的人才能修改。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["UpdateRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/data-scope-objects": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询数据范围对象目录
         * @description 可以单独设置范围等级的对象。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["DataScopeObjectItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/permissions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询权限点目录
         * @description 全部模块声明的权限点，按模块与分组排好。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PermissionItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles/{id}/data-scopes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 整体替换角色的数据范围
         * @description 默认等级加各范围对象单独设置的等级；范围对象必须在目录里。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetRoleDataScopesRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/roles/{id}/permissions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 整体替换角色的权限点
         * @description 只接受权限点目录里存在的编码。内置的系统管理员角色拥有全部权限点，不能手工修改。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 角色 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetRolePermissionsRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfRoleId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/security-events": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询安全事件
         * @description 登录失败、越权访问、授权配置变更、模拟身份这些。时间范围必填，最长 31 天；默认按时间倒序，分页。查询本身是敏感操作，会记读审计。
         */
        get: {
            parameters: {
                query: {
                    /** @description 起始时间，含；必填 */
                    From: string;
                    /** @description 结束时间，不含；必填，与起始时间相隔最长 31 天 */
                    To: string;
                    /** @description 按类型筛选，如 LoginFailed、AuthorizationDenied、AuthorizationConfigChanged */
                    Type?: string;
                    /** @description 按用户筛选 */
                    UserId?: string;
                    /** @description 按客户端 IP 筛选 */
                    ClientIp?: string;
                    /** @description 按登录失败时输入的账号筛选 */
                    Account?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -occurredAt,type：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfSecurityEventItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询用户列表
         * @description 账号、姓名、工号、手机号、状态、所属组织和拥有的角色，分页。系统账号不在列表里。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在账号、姓名、英文名、工号、手机号里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，重复参数名可以给多个，如 status=Active&amp;status=Disabled */
                    Status?: string[];
                    /** @description 只看拥有这个角色的用户 */
                    RoleId?: string;
                    /** @description 只看直接属于这个组织单元的用户 */
                    OrgUnitId?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 displayName,-account：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfUserListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        /**
         * 新建用户
         * @description 不设置密码、所属组织和角色，这些用各自的接口设置。账号、手机号、工号重复分别返回对应的 409。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["CreateUserRequest"];
                };
            };
            responses: {
                /** @description 已创建，Location 指向新数据，响应体是新数据的 ID 与版本号 */
                201: {
                    headers: {
                        /** @description 新数据的地址 */
                        Location?: string;
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CreatedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/disable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 禁用用户
         * @description 他的全部会话随之作废。不能禁用自己，也不能禁用最后一个启用的系统管理员。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["DisableUserRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/enable": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** 启用用户 */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["EnableUserRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/force-logout": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 强制下线
         * @description 指定的一个会话，或该用户的全部会话（含他发起的模拟会话）。不能对自己用，自己的登录设备在“我的登录设备”里管理。持有受保护角色的用户，只有同样持有受保护角色的人才能用。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["ForceLogoutRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询用户详情
         * @description 资料、角色授予与锚点、登录被限制到什么时候、有效会话数、版本号和当前用户可以对他执行的动作。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["UserDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        /**
         * 修改用户资料
         * @description 整体提交资料字段。账号和手机号是登录凭据，对持有受保护角色的用户，只有同样持有受保护角色的人才能改这两项。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["UpdateUserProfileRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/grants": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 授予角色
         * @description 角色必须启用，锚点必须是启用的组织单元。不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能授予。必须带幂等键。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["GrantRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/sessions": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询用户当前有效的会话
         * @description 设备、IP、登录时间和最近刷新时间，含他发起的模拟会话。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["UserSessionItem"][];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/reset-password": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 重置用户密码
         * @description 生成一次性临时密码，只在这次响应里出现；用户的全部会话作废，下次登录必须先改密码。不需要幂等键：重复提交会重新生成，之前的临时密码随之失效。持有受保护角色的用户，只有同样持有受保护角色的人才能重置。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["ResetUserPasswordResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/grants/{roleId}/revoke": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 收回角色
         * @description 正在用这个角色的会话，下一个请求起得到 403，要重新选择角色。不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能收回，也不能收回最后一个启用的系统管理员的角色。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                    /** @description 角色 ID */
                    roleId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RevokeRoleRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/user-options": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 选人下拉
         * @description 登录即可用。只返回姓名、工号、所属组织和状态，不含手机号和邮箱；按姓名排序，分页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在姓名、英文名、工号、账号里模糊匹配 */
                    Q?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfUserOption"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/grants/{roleId}/anchors": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * 整体替换授予上的锚点
         * @description 规则同授予：不能改自己的，受保护角色的要求当前角色也是受保护角色；锚点必须是启用的组织单元。
         */
        put: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                    /** @description 角色 ID */
                    roleId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SetGrantAnchorsRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/identity/users/{id}/unlock-login": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 解锁登录
         * @description 清零这个账号的登录失败计数。登录限制只影响新的登录尝试，不影响用户已有的会话。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 用户 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfUserId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}/attachments": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 添加示例标本的附件
         * @description 文件要先经上传端点上传（上传档案 Sample.SpecimenAttachment），再用得到的文件 ID 添加。只有负责人可以添加，已关闭的标本不能添加；文件必须是本人上传、没有过期的临时文件。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["AddSpecimenAttachmentRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSpecimenId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}/close": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 关闭示例标本
         * @description 已提交的标本关闭。只有负责人可以关闭。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["CloseSpecimenRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSpecimenId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询示例标本列表
         * @description 按当前用户的数据范围返回标本，分页。页码超过末页返回空列表，每页数量超出 1 到 200 返回 400。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在编号和标题里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，重复参数名可以给多个，如 status=Draft&amp;status=Submitted */
                    Status?: string[];
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -createdAt,number：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段 */
                    Sort?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["PagedResultOfSpecimenListItem"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        /**
         * 创建示例标本
         * @description 创建一个草稿标本，负责人是当前用户。必须带幂等键，超时重试用同一个键。成功返回 201，Location 指向新标本。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path?: never;
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["CreateSpecimenRequest"];
                };
            };
            responses: {
                /** @description 已创建，Location 指向新数据，响应体是新数据的 ID 与版本号 */
                201: {
                    headers: {
                        /** @description 新数据的地址 */
                        Location?: string;
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["CreatedResponseOfSpecimenId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 查询示例标本详情
         * @description 返回标本、明细、并发版本号和当前用户现在可以执行的动作。看不到的标本和不存在的一样，返回 404。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["SpecimenDetailResponse"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        /**
         * 删除示例标本
         * @description 只能删除草稿，软删除。成功返回 204。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["DeleteSpecimenRequest"];
                };
            };
            responses: {
                /** @description 成功，没有响应体 */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}/attachments/{attachmentId}/content": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 下载示例标本的附件
         * @description 权限完全继承标本：看不到的标本和不存在的一样，返回 404。默认作为附件下载；PDF 和图片加 inline=true 可以在浏览器里直接打开。支持 Range 和 ETag。每次下载都有审计记录。文件还没准备好返回 409，已被彻底删除返回 410。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 在浏览器里直接打开；只有 PDF 和图片有效 */
                    inline?: boolean;
                };
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                    /** @description 附件 ID */
                    attachmentId: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["FileDownload"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}/attachments/{attachmentId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post?: never;
        /**
         * 移除示例标本的附件
         * @description 只有负责人可以移除，已关闭的标本不能移除。文件解除关联后保留一段时间，期间可以重新关联。成功返回 204。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 标本 ID */
                    id: string;
                    /** @description 附件 ID */
                    attachmentId: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["RemoveSpecimenAttachmentRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSpecimenId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/sample/specimens/{id}/submit": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * 提交示例标本
         * @description 草稿提交为已提交。只有负责人可以提交，至少要有一项明细。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 标本 ID */
                    id: string;
                };
                cookie?: never;
            };
            requestBody: {
                content: {
                    "application/json": components["schemas"]["SubmitSpecimenRequest"];
                };
            };
            responses: {
                /** @description 成功 */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["VersionedResponseOfSpecimenId"];
                    };
                };
                /** @description 请求格式不正确，或者提交的数据没有通过校验，具体见 errors */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有登录，或者登录已失效 */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 没有执行这个操作的权限 */
                403: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据不存在，或者当前用户看不到 */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion */
                409: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 违反了业务规则，原因见 code 和 title */
                422: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 请求太频繁，或者系统正忙，按 Retry-After 等待后再试 */
                429: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
                /** @description 系统出错了，原因没有写在响应里，报障时提供 traceId */
                500: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/problem+json": components["schemas"]["KmicProblem"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        /** @description 放弃一条失败投递的请求 */
        AbandonDeliveryRequest: {
            /** @description 放弃的原因，必填，最多 500 个字符 */
            reason: string;
        };
        /** @description 放弃死信的请求 */
        AbandonMessagesRequest: {
            /** @description 要放弃的消息，最多 200 条，都在收件箱 */
            messages: components["schemas"]["MessageRef"][];
            /** @description 原因，必填，最多 500 个字符 */
            reason: string;
        };
        /** @description 添加字典项的请求 */
        AddDictionaryItemRequest: {
            /**
             * Format: int64
             * @description 调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 编码，字母、数字、点、短横线、下划线，1 到 50 位，创建后不能改 */
            code: string;
            /** @description 显示名，最多 200 个字符 */
            name: string;
            /** @description 父项编码；只有层级字典可以给 */
            parentCode: null | string;
            /**
             * Format: int32
             * @description 次序；不给就排在同级最后
             */
            sortOrder: null | number;
        };
        /** @description 添加标本附件的请求 */
        AddSpecimenAttachmentRequest: {
            /**
             * Format: uuid
             * @description 上传得到的文件 ID
             */
            fileId: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 设置用户所属组织单元的请求 */
        AssignUserOrgUnitRequest: {
            /**
             * Format: int64
             * @description 调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 组织单元，必须启用；不给表示不属于任何单元
             */
            orgUnitId: string;
        };
        /** @description 审计记录的详情 */
        AuditDetail: {
            /**
             * Format: uuid
             * @description 审计记录 ID
             */
            id: string;
            /**
             * Format: date-time
             * @description 操作发生的时间
             */
            occurredAt: string;
            /**
             * Format: date-time
             * @description 汇总到这里的时间
             */
            receivedAt: string;
            /** @description 产生审计的模块 */
            module: string;
            /** @description 操作名 */
            action: string;
            /** @description 请求来源，如 Http、ScheduledJob */
            origin: string;
            /**
             * Format: uuid
             * @description 操作人
             */
            actorUserId: null | string;
            /** @description 操作人的名称；用户已经不存在时为空 */
            actorName: null | string;
            /**
             * Format: uuid
             * @description 模拟登录时真正动手的人
             */
            actualUserId: null | string;
            /** @description 实际操作人的名称 */
            actualName: null | string;
            /**
             * Format: uuid
             * @description 操作时使用的角色
             */
            roleId: null | string;
            /** @description 角色名称；角色已经不存在时为空 */
            roleName: null | string;
            /**
             * Format: uuid
             * @description 会话
             */
            sessionId: null | string;
            /**
             * Format: uuid
             * @description 发起原始请求的用户
             */
            onBehalfOfUserId: null | string;
            /** @description 客户端 IP */
            clientIp: null | string;
            /** @description 链路追踪 ID，可以到日志与链路里查这次操作 */
            traceId: string;
            /** @description 操作对象 */
            targets: components["schemas"]["AuditTargetItem"][];
            /** @description 字段变更：实体、变更类型、各字段新旧值，敏感字段已脱敏 */
            changes: unknown;
            /** @description 补充说明，如导出的筛选条件 */
            details: unknown;
        };
        /** @description 审计列表里的一条 */
        AuditListItem: {
            /**
             * Format: uuid
             * @description 审计记录 ID
             */
            id: string;
            /**
             * Format: date-time
             * @description 操作发生的时间
             */
            occurredAt: string;
            /** @description 产生审计的模块 */
            module: string;
            /** @description 操作名 */
            action: string;
            /** @description 请求来源，如 Http、ScheduledJob */
            origin: string;
            /**
             * Format: uuid
             * @description 操作人
             */
            actorUserId: null | string;
            /** @description 操作人的名称；用户已经不存在时为空 */
            actorName: null | string;
            /**
             * Format: uuid
             * @description 模拟登录时真正动手的人
             */
            actualUserId: null | string;
            /** @description 实际操作人的名称 */
            actualName: null | string;
            /** @description 链路追踪 ID，可以到日志与链路里查这次操作 */
            traceId: string;
            /** @description 操作对象 */
            targets: components["schemas"]["AuditTargetItem"][];
        };
        /** @description 审计里记的一个操作对象 */
        AuditTargetItem: {
            /** @description 资源类型，如 User */
            resourceType: string;
            /**
             * Format: uuid
             * @description 资源 ID
             */
            resourceId: string;
        };
        /** @description 登录、刷新、切换角色返回的令牌 */
        AuthTokens: {
            /** @description 访问令牌 */
            access_token: string;
            /** @description 固定为 Bearer */
            token_type: string;
            /**
             * Format: int32
             * @description 访问令牌还有多少秒过期
             */
            expires_in: number;
            /** @description 刷新令牌；没有新签发时为空 */
            refresh_token: null | string;
            /** @description 是否必须先修改密码 */
            passwordChangeRequired: boolean;
        };
        /** @description 后台作业列表里的一项：状态、进度、结果摘要、错误，看不到作业参数 */
        BackgroundJobItem: {
            /**
             * Format: uuid
             * @description 作业 ID
             */
            id: string;
            /** @description 作业所在的模块 */
            module: string;
            /** @description 作业类型名，如 Sample.ExportSpecimens */
            jobType: string;
            /** @description 状态 */
            status: components["schemas"]["BackgroundJobStatus"];
            /**
             * Format: uuid
             * @description 提交人；系统提交的没有
             */
            submittedBy: null | string;
            /** @description 提交人的名称；用户已经不存在时为空 */
            submittedByName: null | string;
            /**
             * Format: date-time
             * @description 提交时间
             */
            submittedAt: string;
            /**
             * Format: date-time
             * @description 第一次开始执行的时间
             */
            startedAt: string;
            /**
             * Format: date-time
             * @description 结束时间
             */
            finishedAt: string;
            /**
             * Format: int64
             * @description 已完成数量
             */
            progressDone: number;
            /**
             * Format: int64
             * @description 总数量，不知道时为空
             */
            progressTotal: null | number;
            /** @description 进度说明 */
            progressMessage: null | string;
            /**
             * Format: uuid
             * @description 结果文件
             */
            resultFileId: null | string;
            /** @description 结果摘要，JSON；取消不是回滚，已提交的批次保留，摘要里说明处理了多少 */
            resultSummary: unknown;
            /** @description 失败时的错误码 */
            errorCode: null | string;
            /** @description 失败时的错误说明 */
            errorMessage: null | string;
            /**
             * Format: int32
             * @description 执行次数，续跑会加一
             */
            attempts: number;
            /**
             * Format: date-time
             * @description 请求取消的时间
             */
            cancelRequestedAt: string;
            /**
             * Format: uuid
             * @description 请求取消的人
             */
            cancelRequestedBy: null | string;
            /** @description 请求取消的人的名称 */
            cancelRequestedByName: null | string;
        };
        /**
         * @description 后台作业的状态
         * @enum {string}
         */
        BackgroundJobStatus: "Queued" | "Running" | "Succeeded" | "Failed" | "Cancelling" | "Cancelled";
        /** @description 一个被死信挡住的顺序键 */
        BlockedKeyItem: {
            /** @description 订阅方模块 */
            module: string;
            /** @description 有序订阅的名称 */
            subscription: string;
            /** @description 顺序键，通常是聚合 ID */
            orderingKey: string;
            /**
             * Format: int64
             * @description 这个键下的死信条数
             */
            deadCount: number;
            /**
             * Format: int64
             * @description 排在最早的死信后面、还在等的条数
             */
            waiting: number;
            /**
             * Format: uuid
             * @description 最早的一条死信的行 ID：处理它才能放开这个键，重试与放弃都用它
             */
            firstDeadId: string;
            /** @description 最早的一条死信最近一次的错误 */
            lastError: null | string;
        };
        /** @description 一条捕获内容的详情 */
        CapturedDetailResponse: {
            /**
             * Format: uuid
             * @description 捕获内容
             */
            id: string;
            /**
             * Format: uuid
             * @description 对应的投递
             */
            deliveryId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 本该发往的完整地址 */
            address: string;
            /** @description 邮件里的角色 */
            role: components["schemas"]["DeliveryRole"];
            /** @description 主题 */
            subject: string;
            /** @description 正文 */
            body: string;
            /**
             * Format: date-time
             * @description 捕获的时间
             */
            capturedAt: string;
        };
        /** @description 捕获列表里的一条，不含正文 */
        CapturedListItem: {
            /**
             * Format: uuid
             * @description 捕获内容
             */
            id: string;
            /**
             * Format: uuid
             * @description 对应的投递
             */
            deliveryId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 本该发往的完整地址 */
            address: string;
            /** @description 邮件里的角色 */
            role: components["schemas"]["DeliveryRole"];
            /** @description 主题 */
            subject: string;
            /**
             * Format: date-time
             * @description 捕获的时间
             */
            capturedAt: string;
        };
        /** @description 修改密码的请求 */
        ChangePasswordRequest: {
            /** @description 旧密码；还没有设置过密码时不用给 */
            oldPassword: null | string;
            /** @description 新密码 */
            newPassword: string;
        };
        /** @description 前端上报的一条错误 */
        ClientErrorReport: {
            /** @description 错误类型，如 TypeError、ApiError，最多 100 个字符 */
            type: string;
            /** @description 错误消息，最多 1000 个字符 */
            message: string;
            /** @description 堆栈，生产环境是压缩后的位置，最多 8000 个字符 */
            stack: null | string;
            /** @description 出错时的路由名，不是带参数的地址，最多 200 个字符 */
            routeName: null | string;
            /** @description 前端版本，最多 50 个字符 */
            appVersion: null | string;
            /** @description 相关接口响应里的追踪 ID，用来和后端的追踪关联，32 位十六进制 */
            traceId: null | string;
            /**
             * Format: date-time
             * @description 在浏览器上发生的时间
             */
            occurredAt: string;
        };
        /**
         * @description 客户端类型，决定刷新令牌怎样存放、滑动与绝对有效期多长
         * @enum {string}
         */
        ClientType: "Web" | "App";
        /** @description 关闭示例标本的请求 */
        CloseSpecimenRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 创建结果 */
        CreatedResponseOfOrgUnitId: {
            /**
             * Format: uuid
             * @description 新数据的 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 创建结果 */
        CreatedResponseOfRoleId: {
            /**
             * Format: uuid
             * @description 新数据的 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 创建结果 */
        CreatedResponseOfSpecimenId: {
            /**
             * Format: uuid
             * @description 新数据的 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 创建结果 */
        CreatedResponseOfUserId: {
            /**
             * Format: uuid
             * @description 新数据的 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 添加组织单元的请求 */
        CreateOrgUnitRequest: {
            /**
             * Format: int64
             * @description 调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 上级组织单元，必须启用
             */
            parentId: string;
            /** @description 名称，最多 100 个字符，同一上级下不区分大小写唯一 */
            name: string;
            /** @description 编码，最多 50 个字符，可以不填 */
            code: null | string;
        };
        /** @description 新建角色的请求 */
        CreateRoleRequest: {
            /** @description 名称，最多 100 个字符，不区分大小写唯一 */
            name: string;
            /** @description 说明，最多 500 个字符 */
            description: null | string;
            /**
             * Format: int32
             * @description 登录时角色下拉里的次序，0 到 10000
             */
            sortOrder: number;
            /** @description 默认数据范围等级：Self、OrgUnit、OrgUnitAndBelow、All */
            defaultScopeLevel: components["schemas"]["RoleScopeLevel"];
        };
        /** @description 创建标本时的一项明细 */
        CreateSpecimenItemRequest: {
            /** @description 名称，最多 50 个字符 */
            name: string;
            /**
             * Format: int32
             * @description 数量，1 到 9999
             */
            quantity: number;
        };
        /** @description 创建示例标本的请求 */
        CreateSpecimenRequest: {
            /** @description 标题，最多 100 个字符 */
            title: string;
            /** @description 明细，至少一项，最多 50 项 */
            items: components["schemas"]["CreateSpecimenItemRequest"][];
        };
        /** @description 新建用户的请求 */
        CreateUserRequest: {
            /** @description 登录名，最多 100 个字符，不区分大小写唯一 */
            account: string;
            /** @description 姓名，最多 100 个字符 */
            displayName: string;
            /** @description 英文名，最多 100 个字符 */
            englishName: null | string;
            /** @description 工号，最多 50 个字符，填写时唯一 */
            employeeNo: null | string;
            /** @description 手机号，填写时唯一；国内号码可以不带 +86 */
            mobile: null | string;
            /** @description 邮箱，最多 200 个字符 */
            email: null | string;
        };
        /** @description 一个 Cron 表达式接下来的触发时间 */
        CronPreviewResponse: {
            /** @description 接下来 5 次触发的时刻，按时间顺序 */
            nextFireTimes: unknown[];
        };
        /** @description 游标分页的一页 */
        CursorPageOfAuditListItem: {
            /** @description 这一页的列表项 */
            items: components["schemas"]["AuditListItem"][];
            /** @description 下一页的游标，作为 After 传回；最后一页为空 */
            nextCursor: null | string;
        };
        /** @description 游标分页的一页 */
        CursorPageOfBackgroundJobItem: {
            /** @description 这一页的列表项 */
            items: components["schemas"]["BackgroundJobItem"][];
            /** @description 下一页的游标，作为 After 传回；最后一页为空 */
            nextCursor: null | string;
        };
        /** @description 游标分页的一页 */
        CursorPageOfDeadMessageItem: {
            /** @description 这一页的列表项 */
            items: components["schemas"]["DeadMessageItem"][];
            /** @description 下一页的游标，作为 After 传回；最后一页为空 */
            nextCursor: null | string;
        };
        /** @description 游标分页的一页 */
        CursorPageOfScheduledJobRunItem: {
            /** @description 这一页的列表项 */
            items: components["schemas"]["ScheduledJobRunItem"][];
            /** @description 下一页的游标，作为 After 传回；最后一页为空 */
            nextCursor: null | string;
        };
        /** @description 数据范围对象目录里的一项 */
        DataScopeObjectItem: {
            /** @description 编码 */
            code: string;
            /** @description 所属模块 */
            module: string;
            /** @description 显示名 */
            displayName: string;
        };
        /** @description 一条死信的全部内容：概要加载荷与信封 */
        DeadMessageDetailItem: {
            /** @description 概要 */
            message: components["schemas"]["DeadMessageItem"];
            /** @description 状态：死信，或者已经被放弃 */
            state: components["schemas"]["MessageState"];
            /** @description 载荷。按设计不含敏感字段，含密消息（验证码之类）的载荷不进这里；每次查看都会留下读取审计 */
            payload: unknown;
            /** @description 信封里的其余字段 */
            headers: unknown;
        };
        /** @description 死信列表里的一条 */
        DeadMessageItem: {
            /**
             * Format: uuid
             * @description 行 ID；重试、放弃、查看内容都用它。收件箱里是收件箱行的 ID，不是消息 ID
             */
            id: string;
            /** @description 在发件箱还是收件箱 */
            box: components["schemas"]["MessageBoxKind"];
            /** @description 所在模块 */
            module: string;
            /** @description 订阅名；发件箱里没有 */
            subscription: null | string;
            /**
             * Format: uuid
             * @description 消息 ID
             */
            messageId: string;
            /** @description 消息名称 */
            messageType: string;
            /**
             * Format: int32
             * @description 消息版本
             */
            schemaVersion: number;
            /**
             * Format: date-time
             * @description 消息发生的时间
             */
            occurredAt: string;
            /**
             * Format: int32
             * @description 已经试了几次
             */
            attempts: number;
            /** @description 最近一次的错误 */
            lastError: null | string;
        };
        /** @description 删除示例标本的请求 */
        DeleteSpecimenRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /**
         * @description 投递的渠道：站内消息不进投递队列，所以这里只有邮件和短信
         * @enum {string}
         */
        DeliveryChannel: "Email" | "Sms";
        /** @description 一条投递的详情 */
        DeliveryDetailResponse: {
            /**
             * Format: uuid
             * @description 投递
             */
            id: string;
            /**
             * Format: uuid
             * @description 产生它的发送请求
             */
            notificationId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 邮件里的角色 */
            role: components["schemas"]["DeliveryRole"];
            /** @description 状态 */
            status: components["schemas"]["DeliveryStatus"];
            skipReason: null | components["schemas"]["DeliverySkipReason"];
            /**
             * Format: uuid
             * @description 收件用户，直接给地址的收件人为空
             */
            recipientUserId: string;
            /** @description 脱敏后的收件地址 */
            addressMasked: string;
            /** @description 渲染后的主题 */
            subject: string;
            /** @description 渲染后的正文；含密模板发完或最终失败后为空 */
            body: null | string;
            /** @description 模板键 */
            templateKey: string;
            /**
             * Format: int32
             * @description 用的模板版本号，0 表示代码里的默认内容
             */
            templateVersion: number;
            /** @description 优先级 */
            priority: components["schemas"]["DeliveryPriority"];
            /**
             * Format: int32
             * @description 已经尝试发送几次
             */
            attempts: number;
            /**
             * Format: date-time
             * @description 下次可以领取的时间
             */
            nextAttemptAt: string;
            /** @description 最近一次失败的原因 */
            lastError: null | string;
            /** @description 渠道返回的消息标识 */
            providerMessageId: null | string;
            /**
             * Format: date-time
             * @description 发送成功的时间
             */
            sentAt: string;
            /**
             * Format: date-time
             * @description 创建时间
             */
            createdAt: string;
            /**
             * Format: date-time
             * @description 放弃处理的时间，没放弃为空
             */
            abandonedAt: string;
            /** @description 放弃处理的原因 */
            abandonReason: null | string;
        };
        /** @description 投递列表里的一条，不含正文 */
        DeliveryListItem: {
            /**
             * Format: uuid
             * @description 投递
             */
            id: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 状态 */
            status: components["schemas"]["DeliveryStatus"];
            skipReason: null | components["schemas"]["DeliverySkipReason"];
            /**
             * Format: uuid
             * @description 收件用户，直接给地址的收件人为空
             */
            recipientUserId: string;
            /** @description 脱敏后的收件地址 */
            addressMasked: string;
            /** @description 模板键 */
            templateKey: string;
            /**
             * Format: int32
             * @description 用的模板版本号，0 表示代码里的默认内容
             */
            templateVersion: number;
            /**
             * Format: int32
             * @description 已经尝试发送几次
             */
            attempts: number;
            /** @description 最近一次失败的原因 */
            lastError: null | string;
            /**
             * Format: date-time
             * @description 发送成功的时间
             */
            sentAt: string;
            /**
             * Format: date-time
             * @description 创建时间
             */
            createdAt: string;
            /** @description 是否已经放弃处理 */
            abandoned: boolean;
        };
        /**
         * @description 投递的优先级：高优先级的先被领取
         * @enum {string}
         */
        DeliveryPriority: "Normal" | "High";
        /**
         * @description 邮件收件人的角色
         * @enum {string}
         */
        DeliveryRole: "To" | "Cc" | "Bcc";
        /**
         * @description 投递被跳过的原因
         * @enum {string}
         */
        DeliverySkipReason: "NoAddress" | "UserDisabled" | "UnknownUser";
        /**
         * @description 投递的状态
         * @enum {string}
         */
        DeliveryStatus: "Pending" | "Sending" | "Sent" | "Failed" | "Skipped" | "Captured";
        /** @description 一批字典 */
        DictionariesResponse: {
            /** @description 取到的字典快照，按请求里类型的次序 */
            dictionaries: components["schemas"]["DictionarySnapshot"][];
            /** @description 各类型版本号的组合：什么都没变时前端带着它再问，得到 304 */
            eTag: string;
        };
        /** @description 字典里的一项 */
        DictionaryEntry: {
            /** @description 编码，业务数据里存的就是它 */
            code: string;
            /** @description 显示名 */
            name: string;
            /** @description 层级字典里父项的编码；顶层为空 */
            parentCode: null | string;
            /**
             * Format: int32
             * @description 在同一父项下的次序
             */
            sortOrder: number;
            /** @description 是否启用；停用的项历史数据仍然显示，新数据不能再选 */
            isActive: boolean;
        };
        /** @description 停用、启用字典项的请求 */
        DictionaryItemVersionRequest: {
            /**
             * Format: int64
             * @description 调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 一个字典类型的快照：类型信息、版本号和全部字典项（含停用的） */
        DictionarySnapshot: {
            /** @description 类型编码 */
            typeCode: string;
            /** @description 类型显示名 */
            displayName: string;
            /** @description 是否层级字典 */
            isHierarchical: boolean;
            /**
             * Format: int64
             * @description 类型的版本号，任何字典项的变化都会递增它
             */
            version: number;
            /** @description 全部字典项，按父项和次序排好 */
            items: components["schemas"]["DictionaryEntry"][];
        };
        /** @description 字典类型详情 */
        DictionaryTypeDetail: {
            /** @description 类型编码 */
            code: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            ownerModule: string;
            /** @description 是否层级字典 */
            isHierarchical: boolean;
            /** @description 状态：使用中、已退役 */
            status: components["schemas"]["DictionaryTypeStatus"];
            /**
             * Format: int64
             * @description 版本号，输出为 version；修改字典项时带上它做并发检查
             */
            version: number;
            /** @description 全部字典项，按父项和次序排好 */
            items: components["schemas"]["DictionaryEntry"][];
        };
        /** @description 字典类型列表的一行 */
        DictionaryTypeListItem: {
            /** @description 类型编码 */
            code: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            ownerModule: string;
            /** @description 是否层级字典 */
            isHierarchical: boolean;
            /** @description 状态：使用中、已退役 */
            status: components["schemas"]["DictionaryTypeStatus"];
            /**
             * Format: int32
             * @description 字典项总数，含停用的
             */
            itemCount: number;
            /**
             * Format: int32
             * @description 启用的字典项数
             */
            activeItemCount: number;
            /**
             * Format: int64
             * @description 版本号，输出为 version
             */
            version: number;
        };
        /**
         * @description 字典类型的状态
         * @enum {string}
         */
        DictionaryTypeStatus: "Active" | "Retired";
        /** @description 停用角色的请求 */
        DisableRoleRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 原因，记入状态历史 */
            reason: null | string;
        };
        /** @description 禁用用户的请求 */
        DisableUserRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 原因，记入状态历史 */
            reason: null | string;
        };
        /** @description 启用角色的请求 */
        EnableRoleRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 启用用户的请求 */
        EnableUserRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 提交审计导出的请求 */
        ExportAuditRequest: {
            /**
             * Format: date-time
             * @description 起始时间，含；必填
             */
            from: string;
            /**
             * Format: date-time
             * @description 结束时间，不含；必填
             */
            to: string;
            /**
             * Format: uuid
             * @description 操作人
             */
            actorUserId?: string;
            /**
             * Format: uuid
             * @description 实际操作人：模拟登录时真正动手的人
             */
            actualUserId?: string;
            /** @description 产生审计的模块，如 Identity */
            module?: null | string;
            /** @description 操作名，如 Identity.CreateUser */
            action?: null | string;
            /** @description 操作对象的类型，如 User */
            resourceType?: null | string;
            /**
             * Format: uuid
             * @description 操作对象的 ID
             */
            resourceId?: null | string;
        };
        /** @description 当前对请求开放的功能开关 */
        FeaturesResponse: {
            /** @description 开启的开关名，按名称排序；没有配置的开关按关闭处理，不在列表里 */
            enabled: string[];
        };
        /** @description 文件的详情：元数据、状态和引用，不含存储键和内容 */
        FileDetail: {
            /**
             * Format: uuid
             * @description 文件
             */
            id: string;
            /**
             * Format: int64
             * @description 并发版本号，修改时作为 expectedVersion 带回
             */
            version: number;
            /** @description 显示用的文件名 */
            name: string;
            /** @description 文件类型编码 */
            fileType: null | string;
            /** @description 内容类型 */
            contentType: null | string;
            /**
             * Format: int64
             * @description 字节数
             */
            size: null | number;
            /** @description 内容的 SHA-256 */
            sha256: null | string;
            /** @description 上传档案 */
            profile: string;
            /**
             * Format: uuid
             * @description 上传的人
             */
            uploadedBy: string;
            /**
             * Format: date-time
             * @description 上传开始的时间
             */
            uploadedAt: string;
            /** @description 状态 */
            status: components["schemas"]["StoredFileStatus"];
            /**
             * Format: date-time
             * @description 成为清理候选的时间
             */
            expiresAt: string;
            /** @description 被拒绝的原因 */
            rejectReason: null | string;
            /**
             * Format: date-time
             * @description 彻底删除的时间
             */
            purgedAt: string;
            /**
             * Format: uuid
             * @description 彻底删除的人
             */
            purgedBy: string;
            /** @description 彻底删除的原因 */
            purgeReason: null | string;
            /**
             * Format: date-time
             * @description 对象在存储里被删除的时间，彻底删除的文件才有
             */
            objectDeletedAt: string;
            /**
             * Format: date-time
             * @description 运维登记彻底删除历史版本的时间，空表示历史版本可能仍在
             */
            historyPurgedAt: string;
            /** @description 彻底删除历史版本的执行记录 */
            historyPurgeNote: null | string;
            /** @description 引用它的业务对象 */
            references: components["schemas"]["FileReferenceItem"][];
            /** @description 状态流转记录，按时间从早到晚 */
            history: components["schemas"]["StateHistoryItem"][];
        };
        /** @description 一次文件下载：业务的下载查询授权、确认关联之后组装出来，端点用 FileResults.Stream 把它写成响应 */
        FileDownload: {
            /**
             * Format: uuid
             * @description 文件 ID
             */
            fileId: string;
            /** @description 对象在存储里的键 */
            storageKey: string;
            /** @description 下载时的文件名 */
            name: string;
            /** @description 内容类型 */
            contentType: string;
            /**
             * Format: int64
             * @description 字节数
             */
            size: number;
            /** @description 内容的 SHA-256，响应的 ETag */
            sha256: string;
            /** @description 文件类型编码，决定能不能在浏览器里直接打开 */
            fileType: string;
        };
        /** @description 文件列表里的一行，不含存储键 */
        FileListItem: {
            /**
             * Format: uuid
             * @description 文件
             */
            id: string;
            /** @description 显示用的文件名 */
            name: string;
            /** @description 文件类型编码，上传没写完的没有 */
            fileType: null | string;
            /**
             * Format: int64
             * @description 字节数，上传没写完的没有
             */
            size: null | number;
            /** @description 上传档案 */
            profile: string;
            /**
             * Format: uuid
             * @description 上传的人，系统生成的文件没有
             */
            uploadedBy: string;
            /**
             * Format: date-time
             * @description 上传开始的时间
             */
            uploadedAt: string;
            /** @description 状态 */
            status: components["schemas"]["StoredFileStatus"];
            /**
             * Format: date-time
             * @description 成为清理候选的时间
             */
            expiresAt: string;
            /**
             * Format: int64
             * @description 并发版本号
             */
            version: number;
        };
        /** @description 一个业务对象对文件的引用 */
        FileReferenceItem: {
            /** @description 引用它的业务模块 */
            ownerModule: string;
            /** @description 业务对象的类型 */
            ownerType: string;
            /**
             * Format: uuid
             * @description 业务对象的 ID
             */
            ownerId: string;
            /**
             * Format: date-time
             * @description 关联的时间
             */
            attachedAt: string;
        };
        /** @description 终结订阅的请求 */
        FinalizeRouteRequest: {
            /** @description 消息名称 */
            messageType: string;
            /** @description 订阅名 */
            subscription: string;
            /** @description 原因，必填 */
            reason: string;
            /**
             * @description 有没处理完的消息时，是否把它们放弃；不给就是不放弃，有未完成消息时拒绝终结
             * @default false
             */
            abandonUnfinished: boolean;
        };
        /** @description 终结订阅的结果 */
        FinalizeRouteResponse: {
            /**
             * Format: int64
             * @description 终结时放弃的没处理完的消息条数
             */
            abandoned: number;
        };
        /** @description 强制下线的请求 */
        ForceLogoutRequest: {
            /**
             * Format: uuid
             * @description 要下线的会话；不给就是该用户的全部会话
             */
            sessionId: string;
        };
        /** @description 授予角色的请求 */
        GrantRoleRequest: {
            /**
             * Format: int64
             * @description 调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 角色，必须启用
             */
            roleId: string;
            /** @description 锚点：这个角色的“本组织”从哪些组织单元算起，必须是启用的组织单元；不给就从用户所属的组织单元算 */
            anchors: null | unknown[];
        };
        /** Format: binary */
        IFormFile: string;
        /** @description 一个实例：构建号、角色、心跳和它声明的订阅 */
        InstanceItem: {
            /**
             * Format: uuid
             * @description 实例 ID
             */
            id: string;
            /** @description 机器名 */
            host: string;
            /** @description 角色 */
            roles: string;
            /** @description 构建号 */
            build: string;
            /**
             * Format: date-time
             * @description 启动时间
             */
            startedAt: string;
            /**
             * Format: date-time
             * @description 最近一次心跳
             */
            heartbeatAt: string;
            /** @description 最近有心跳，当作还在运行 */
            isAlive: boolean;
            /** @description 声明的订阅 */
            subscriptions: components["schemas"]["InstanceSubscription"][];
        };
        /** @description 实例声明的一个订阅 */
        InstanceSubscription: {
            /** @description 消息名称 */
            messageType: string;
            /** @description 订阅名 */
            subscription: string;
            /** @description 订阅方模块 */
            module: string;
        };
        /** @description 后台作业已受理 */
        JobAccepted: {
            /**
             * Format: uuid
             * @description 作业 ID，用它查询进度和结果
             */
            jobId: string;
        };
        /**
         * @description 定时任务一次执行的状态
         * @enum {string}
         */
        JobRunStatus: "Running" | "Succeeded" | "Failed" | "Unknown" | "Abandoned";
        /**
         * @description 定时任务这次执行是怎么触发的
         * @enum {string}
         */
        JobRunTrigger: "Schedule" | "Manual" | "Misfire";
        /** @description 统一的错误响应，格式是 RFC 9457 的 ProblemDetails 加上几个固定的扩展字段 */
        KmicProblem: {
            /** @description 错误类型，urn:kmic:error:{code}，与 code 一一对应 */
            type: string;
            /** @description 给用户看的提示文字 */
            title: string;
            /**
             * Format: int32
             * @description HTTP 状态码
             */
            status: number;
            /** @description 补充说明，没有时不输出 */
            detail?: string;
            /** @description 出错的请求路径，不含查询参数 */
            instance?: string;
            /** @description 稳定的错误码，如 Sample.NotOwner；前端按它判断，不按文字判断 */
            code: string;
            /** @description 链路追踪 ID，与响应头 X-Trace-Id 相同，用户报障时提供 */
            traceId: string;
            /** @description 原样重试是否可能成功 */
            retryable: boolean;
            /**
             * Format: int64
             * @description 数据的当前版本号，只在并发冲突时输出
             */
            currentVersion?: number;
            /** @description 字段错误，校验失败时输出，每条对应请求中的一个字段 */
            errors?: {
                /** @description 字段路径，与请求体里的写法一致，如 items[2].quantity */
                path: string;
                /** @description 校验规则，如 Required、Range、Format */
                code: string;
                /** @description 给用户看的提示文字 */
                message: string;
            }[];
        };
        /** @description 登录请求 */
        LoginRequest: {
            /** @description 登录名 */
            account: string;
            /** @description 密码 */
            password: string;
            /** @description 客户端类型：web 或 app */
            clientType: string;
        };
        /** @description 标为已读的结果 */
        MarkedReadResponse: {
            /**
             * Format: int32
             * @description 这次标了几条
             */
            count: number;
        };
        /** @description 当前登录用户的信息 */
        MeResponse: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            userId: string;
            /** @description 账号 */
            account: string;
            /** @description 姓名 */
            displayName: string;
            /** @description 英文名 */
            englishName: null | string;
            /**
             * Format: uuid
             * @description 所属组织单元
             */
            orgUnitId: null | string;
            /** @description 当前角色 */
            currentRole: components["schemas"]["MeRole"];
            /** @description 可以切换的角色（启用的） */
            roles: components["schemas"]["MeRole"][];
            /** @description 当前角色的权限点，按编码排序 */
            permissions: string[];
            /**
             * Format: int64
             * @description 授权版本号：变化时前端重新拉取本接口
             */
            authorizationVersion: number;
            /** @description 是否必须先修改密码 */
            passwordChangeRequired: boolean;
            /** @description 是否已经设置过密码：没有时修改密码不要求旧密码 */
            hasPassword: boolean;
            /** @description 是否正在模拟身份 */
            impersonating: boolean;
            /**
             * Format: uuid
             * @description 模拟身份时的实际操作人
             */
            impersonatorUserId: null | string;
            /** @description 模拟会话是否允许修改数据 */
            impersonationAllowsWrite: boolean;
            /** @description 业务时区（IANA ID），前端统一按它显示时刻 */
            businessTimeZone: string;
        };
        /** @description 用户可用的一个角色 */
        MeRole: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            id: string;
            /** @description 角色名称 */
            name: string;
        };
        /**
         * @description 消息所在的箱
         * @enum {string}
         */
        MessageBoxKind: "Outbox" | "Inbox";
        /** @description 重试或放弃一条消息的回执：每条各有各的结果，一条失败不影响别的 */
        MessageReceipt: {
            /** @description 所在模块 */
            module: string;
            /** @description 在发件箱还是收件箱 */
            box: components["schemas"]["MessageBoxKind"];
            /**
             * Format: uuid
             * @description 行 ID
             */
            id: string;
            /** @description 结果 */
            status: components["schemas"]["MessageReceiptStatus"];
            /** @description 没做成时的原因：接收方的放弃检查不让放弃时是它给的说明 */
            reason: null | string;
        };
        /** @description 重试或放弃一批消息的回执 */
        MessageReceipts: {
            /** @description 每条一个回执，顺序与请求一致 */
            receipts: components["schemas"]["MessageReceipt"][];
        };
        /**
         * @description 重试或放弃一条消息的结果
         * @enum {string}
         */
        MessageReceiptStatus: "Done" | "NotFound" | "WrongState" | "Refused";
        /** @description 要重试或放弃的一条消息 */
        MessageRef: {
            /** @description 所在模块 */
            module: string;
            /** @description 在发件箱还是收件箱：Outbox 或 Inbox */
            box: components["schemas"]["MessageBoxKind"];
            /**
             * Format: uuid
             * @description 行 ID，来自死信列表
             */
            id: string;
        };
        /**
         * @description 订阅路由的状态
         * @enum {string}
         */
        MessageRouteStatus: "Active" | "Retiring" | "Finalized";
        /**
         * @description 管理视图里一条消息的状态：只有运维关心的两种
         * @enum {string}
         */
        MessageState: "Dead" | "Abandoned";
        /** @description 消息积压汇总 */
        MessageSummaryResponse: {
            /** @description 每个模块一项，按模块名排好 */
            modules: components["schemas"]["ModuleBacklogItem"][];
        };
        /** @description 一个模块的消息积压 */
        ModuleBacklogItem: {
            /** @description 模块名 */
            module: string;
            /** @description 发件箱 */
            outbox: components["schemas"]["OutboxBacklogItem"];
            /** @description 收件箱里每个订阅的情况 */
            subscriptions: components["schemas"]["SubscriptionBacklogItem"][];
        };
        /** @description 移动字典项的请求 */
        MoveDictionaryItemRequest: {
            /**
             * Format: int64
             * @description 调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的父项编码；不给就移到顶层。不能是自己或自己的后代 */
            newParentCode: null | string;
        };
        /** @description 移动组织单元的请求 */
        MoveOrgUnitRequest: {
            /**
             * Format: int64
             * @description 调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 新的上级，必须启用，不能是自己或自己的下级
             */
            newParentId: string;
        };
        /** @description 本人的一个偏好 */
        MyPreferenceItem: {
            /** @description 键 */
            key: string;
            /** @description 显示名称 */
            displayName: string;
            /** @description 是否偏好族里的键：后端不理解它的内容，由前端按版本化格式读取 */
            isFamily: boolean;
            /** @description 生效的值，JSON 对象 */
            value: unknown;
            /** @description 生效的值来自哪一层；没有任何一层设置过时是默认 */
            source: components["schemas"]["PreferenceSource"];
            /** @description 本人能不能改它：声明里不允许用户层的偏好只能由管理员设置 */
            canSet: boolean;
        };
        /** @description 本人的偏好 */
        MyPreferencesResponse: {
            /** @description 按键排序 */
            preferences: components["schemas"]["MyPreferenceItem"][];
        };
        /** @description 自己的一个登录设备 */
        MySessionItem: {
            /**
             * Format: uuid
             * @description 会话 ID
             */
            id: string;
            /** @description 客户端类型 */
            clientType: components["schemas"]["ClientType"];
            /** @description 设备名，取自登录时的 User-Agent */
            deviceName: null | string;
            /** @description 登录时的 IP */
            loginIp: null | string;
            /**
             * Format: date-time
             * @description 登录时间
             */
            createdAt: string;
            /**
             * Format: date-time
             * @description 最近一次刷新令牌的时间
             */
            lastRefreshedAt: string;
            /** @description 是不是当前正在用的这个会话 */
            isCurrent: boolean;
        };
        /** @description 代码里已经不存在的设置留下的值 */
        ObsoleteSettingItem: {
            /** @description 设置键 */
            key: string;
            /** @description 留下的值 */
            value: unknown;
            /**
             * Format: int64
             * @description 版本号，输出为 version；清除时带上它
             */
            version: number;
        };
        /** @description 组织树 */
        OrgTreeResponse: {
            /**
             * Format: int64
             * @description 组织树版本号，输出为 version；修改组织树的命令用它做并发检查
             */
            version: number;
            /** @description 全部单元，平铺；按上级分组、同一上级下按次序排好，前端按 parentId 拼成树 */
            units: components["schemas"]["OrgUnitItem"][];
        };
        /** @description 组织树上的一个单元 */
        OrgUnitItem: {
            /**
             * Format: uuid
             * @description 组织单元 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 上级；根单元为空
             */
            parentId: string;
            /** @description 名称 */
            name: string;
            /** @description 编码 */
            code: null | string;
            /** @description 状态 */
            status: components["schemas"]["OrgUnitStatus"];
            /**
             * Format: int32
             * @description 在同一上级下的次序
             */
            sortOrder: number;
        };
        /**
         * @description 组织单元的状态
         * @enum {string}
         */
        OrgUnitStatus: "Active" | "Disabled";
        /** @description 停用、启用组织单元的请求 */
        OrgUnitVersionRequest: {
            /**
             * Format: int64
             * @description 调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 发件箱的积压 */
        OutboxBacklogItem: {
            /**
             * Format: int64
             * @description 还没投递完的条数
             */
            pending: number;
            /**
             * Format: int64
             * @description 最老的一条已经发布了多少秒；没有积压时为 0
             */
            oldestAgeSeconds: number;
            /**
             * Format: int64
             * @description 死信条数
             */
            dead: number;
        };
        /** @description 一页数据 */
        PagedResultOfCapturedListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["CapturedListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfDeliveryListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["DeliveryListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfFileListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["FileListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfRoleListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["RoleListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfSecurityEventItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["SecurityEventItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfSettingChangeItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["SettingChangeItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfSiteMessageItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["SiteMessageItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfSpecimenListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["SpecimenListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfStorageIssueItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["StorageIssueItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfUserListItem: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["UserListItem"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 一页数据 */
        PagedResultOfUserOption: {
            /** @description 本页的数据，页码超过末页时为空 */
            items: components["schemas"]["UserOption"][];
            /**
             * Format: int32
             * @description 页码
             */
            page: number;
            /**
             * Format: int32
             * @description 每页数量
             */
            pageSize: number;
            /**
             * Format: int64
             * @description 总数；上限计数且超过上限时等于上限
             */
            totalCount: number;
            /** @description 总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+” */
            totalCountExact: boolean;
        };
        /** @description 权限点目录里的一项 */
        PermissionItem: {
            /** @description 编码 */
            code: string;
            /** @description 所属模块 */
            module: string;
            /** @description 分组 */
            group: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 是否敏感：用到它的命令必须审计，界面单独提示 */
            sensitive: boolean;
        };
        /** @description 管理页上的一个偏好 */
        PreferenceDefinitionItem: {
            /** @description 键 */
            key: string;
            /** @description 显示名称 */
            displayName: string;
            /** @description 所属模块；偏好族里的键属于框架，为空 */
            module: null | string;
            /** @description 是否偏好族里的键 */
            isFamily: boolean;
            /** @description 允许设置的层，优先级由高到低 */
            layers: unknown[];
            /** @description 代码里的默认值；偏好族里的键没有 */
            defaultValue: unknown;
            systemLayer: null | components["schemas"]["PreferenceLayerValue"];
            /** @description 各角色层现有的值 */
            roleLayers: components["schemas"]["PreferenceLayerValue"][];
        };
        /** @description 偏好清单 */
        PreferenceDefinitionsResponse: {
            /** @description 按所属模块、键排序 */
            preferences: components["schemas"]["PreferenceDefinitionItem"][];
        };
        /** @description 偏好在系统层或某个角色层上的值 */
        PreferenceLayerValue: {
            /** @description 值，JSON 对象 */
            value: unknown;
            /**
             * Format: int64
             * @description 版本号，修改或删除时带回来
             */
            version: number;
            /**
             * Format: uuid
             * @description 角色层所属的角色；系统层为空
             */
            roleId: null | string;
            /** @description 角色名称；角色已经不存在时为空 */
            roleName: null | string;
        };
        /**
         * @description 一个偏好当前生效的值来自哪里
         * @enum {string}
         */
        PreferenceSource: "Default" | "System" | "Role" | "UserRole";
        /** @description 预览模板内容的请求 */
        PreviewTemplateRequest: {
            /** @description 渠道：Email、Sms、InApp，必须是模板声明过的 */
            channel: components["schemas"]["TemplateChannel"];
            /** @description 主题 */
            subject: string;
            /** @description 正文 */
            body: string;
            /** @description 用来渲染的数据；不传就用模板的示例数据，含密模板只能用示例数据 */
            model?: unknown;
        };
        /** @description 发布模板新版本的请求 */
        PublishTemplateRequest: {
            /** @description 渠道：Email、Sms、InApp，必须是模板声明过的 */
            channel: components["schemas"]["TemplateChannel"];
            /** @description 主题；站内消息里是标题，最多 200 个字符 */
            subject: string;
            /** @description 正文，最多 20000 个字符 */
            body: string;
            /** @description 备注，最多 500 个字符 */
            remark?: null | string;
            /**
             * Format: int64
             * @description 调用方看到的模板版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion?: null | number;
        };
        /** @description 彻底删除文件的请求 */
        PurgeFileRequest: {
            /** @description 原因，必填 */
            reason: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 重新激活订阅的请求 */
        ReactivateRouteRequest: {
            /** @description 消息名称 */
            messageType: string;
            /** @description 订阅名 */
            subscription: string;
        };
        /** @description 读模型重建的一轮 */
        RebuildItem: {
            /** @description 订阅方模块 */
            module: string;
            /**
             * Format: uuid
             * @description 轮次 ID
             */
            runId: string;
            /** @description 订阅名 */
            subscription: string;
            /** @description 读模型类型名 */
            readModel: string;
            /**
             * Format: uuid
             * @description 执行这一轮的后台作业
             */
            jobId: string;
            /** @description 状态 */
            status: components["schemas"]["RebuildRunStatus"];
            /**
             * Format: int64
             * @description 已扫描的行数
             */
            scanned: number;
            /**
             * Format: int64
             * @description 源端确认不存在的行数，暂停等确认时是已发现的数量
             */
            missing: number;
            /**
             * Format: date-time
             * @description 开始时间
             */
            startedAt: string;
            /**
             * Format: date-time
             * @description 结束时间
             */
            finishedAt: string;
        };
        /** @description 触发、确认、放弃读模型重建的请求 */
        RebuildRequest: {
            /** @description 订阅名 */
            subscription: string;
        };
        /**
         * @description 读模型重建一轮的状态
         * @enum {string}
         */
        RebuildRunStatus: "Active" | "AwaitingConfirmation" | "Completed" | "Abandoned" | "Failed";
        /** @description 登记彻底删除历史版本的请求 */
        RecordHistoryPurgeRequest: {
            /** @description 执行记录，如工单号，必填 */
            note: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 补投的请求 */
        RedeliverRequest: {
            /** @description 发布消息的模块 */
            sourceModule: string;
            /** @description 消息名称 */
            messageType: string;
            /** @description 订阅名 */
            subscription: string;
            /**
             * Format: int64
             * @description 起始序号，含，是来源模块发件箱里的序号
             */
            fromSeq: number;
            /**
             * Format: int64
             * @description 结束序号，含
             */
            toSeq: number;
        };
        /** @description 补投的结果 */
        RedeliverResponse: {
            /**
             * Format: int64
             * @description 区间里符合条件（已经投递过）的消息条数
             */
            candidates: number;
            /**
             * Format: int64
             * @description 真正写进订阅收件箱的条数，订阅已经收到过的不再写
             */
            written: number;
        };
        /** @description 引用信息 */
        ReferenceInfo: {
            /**
             * Format: uuid
             * @description 被引用对象的 ID
             */
            id: string;
            /** @description 显示名称 */
            displayName: string;
            /** @description 编码 */
            code: null | string;
            /** @description 当前状态 */
            status: components["schemas"]["ReferenceStatus"];
        };
        /**
         * @description 引用状态
         * @enum {unknown}
         */
        ReferenceStatus: "Active" | "Disabled" | "Deleted";
        /** @description 刷新请求：移动端在请求体里给出刷新令牌，网页客户端用 Cookie，不需要请求体 */
        RefreshRequest: {
            /** @description 刷新令牌 */
            refreshToken: null | string;
        };
        /** @description 移除标本附件的请求 */
        RemoveSpecimenAttachmentRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 修改字典项显示名的请求 */
        RenameDictionaryItemRequest: {
            /**
             * Format: int64
             * @description 调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的显示名，最多 200 个字符 */
            name: string;
        };
        /** @description 调整字典项次序的请求 */
        ReorderDictionaryItemsRequest: {
            /**
             * Format: int64
             * @description 调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 父项编码；顶层不给 */
            parentCode: null | string;
            /** @description 按新次序排好的该父项下的全部字典项编码，不能多也不能少，不能重复 */
            orderedCodes: string[];
        };
        /** @description 调整同级次序的请求 */
        ReorderOrgUnitsRequest: {
            /**
             * Format: int64
             * @description 调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 按新次序排好的全部直接下级，不能多也不能少，不能重复 */
            orderedChildren: unknown[];
        };
        /** @description 前端批量上报错误的请求 */
        ReportClientErrorsRequest: {
            /** @description 一批错误；一次最多 20 条，更多的拆成几次。字段白名单，未知字段整个请求被拒绝 */
            errors: components["schemas"]["ClientErrorReport"][];
        };
        /** @description 删除偏好在系统层或角色层的值 */
        ResetPreferenceLayerRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过或已经删除就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 重置密码的结果 */
        ResetUserPasswordResponse: {
            /**
             * Format: uuid
             * @description 用户
             */
            id: string;
            /** @description 一次性临时密码，只在这次响应里出现，不写日志、不进审计内容 */
            temporaryPassword: string;
        };
        /** @description 标记存储问题已解决的请求 */
        ResolveStorageIssueRequest: {
            /** @description 处理说明，必填，如还原的工单号 */
            note: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 恢复定时任务默认值的请求 */
        RestoreJobDefaultRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号（列表里的 overrideVersion）。别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 重试死信的请求 */
        RetryMessagesRequest: {
            /** @description 要重试的消息，最多 200 条 */
            messages: components["schemas"]["MessageRef"][];
        };
        /** @description 恢复模板默认内容的请求 */
        RevertTemplateRequest: {
            /** @description 渠道：Email、Sms、InApp，必须是模板声明过的 */
            channel: components["schemas"]["TemplateChannel"];
            /**
             * Format: int64
             * @description 调用方看到的模板版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion?: null | number;
        };
        /** @description 收回角色的请求 */
        RevokeRoleRequest: {
            /**
             * Format: int64
             * @description 调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 角色详情里一个范围对象的单独设置 */
        RoleDataScopeItem: {
            /** @description 范围对象编码 */
            scopeObject: string;
            /** @description 显示名；对象已退役时为空 */
            displayName: null | string;
            /** @description 等级 */
            level: components["schemas"]["RoleScopeLevel"];
        };
        /** @description 角色详情 */
        RoleDetailResponse: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 版本号，输出为 version，也是授权版本号
             */
            version: number;
            /** @description 名称 */
            name: string;
            /** @description 说明 */
            description: null | string;
            /** @description 状态 */
            status: components["schemas"]["RoleStatus"];
            /** @description 是否受保护角色 */
            isProtected: boolean;
            /** @description 是否内置（拥有全部权限点，权限点与数据范围不能手工改） */
            isBuiltIn: boolean;
            /**
             * Format: int32
             * @description 次序
             */
            sortOrder: number;
            /** @description 默认数据范围等级 */
            defaultScopeLevel: components["schemas"]["RoleScopeLevel"];
            /** @description 拥有的权限点；内置角色拥有目录里的全部，这里为空 */
            permissions: components["schemas"]["RolePermissionItem"][];
            /** @description 按范围对象单独设置的等级 */
            dataScopes: components["schemas"]["RoleDataScopeItem"][];
            /**
             * Format: int32
             * @description 持有人数
             */
            holderCount: number;
            /** @description 当前用户现在可以对它执行的动作 */
            allowedActions: string[];
        };
        /** @description 角色列表的一行 */
        RoleListItem: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            id: string;
            /** @description 名称 */
            name: string;
            /** @description 说明 */
            description: null | string;
            /** @description 状态 */
            status: components["schemas"]["RoleStatus"];
            /** @description 是否受保护角色 */
            isProtected: boolean;
            /** @description 是否内置（拥有全部权限点） */
            isBuiltIn: boolean;
            /**
             * Format: int32
             * @description 次序
             */
            sortOrder: number;
            /**
             * Format: int32
             * @description 持有人数
             */
            holderCount: number;
            /**
             * Format: int64
             * @description 版本号，输出为 version
             */
            version: number;
        };
        /** @description 角色详情里的一个权限点 */
        RolePermissionItem: {
            /** @description 权限点编码 */
            code: string;
            /** @description 显示名；已退役的权限点目录里没有，为空 */
            displayName: null | string;
            /** @description 是否已退役：代码里已经没有这个权限点，替换权限点时会去掉 */
            retired: boolean;
        };
        /**
         * @description 角色上配置的数据范围等级
         * @enum {string}
         */
        RoleScopeLevel: "Self" | "OrgUnit" | "OrgUnitAndBelow" | "All";
        /**
         * @description 角色的状态
         * @enum {string}
         */
        RoleStatus: "Active" | "Disabled";
        /** @description 声明某个订阅的一个构建，和运行着它的实例数 */
        RouteBuild: {
            /** @description 构建号 */
            build: string;
            /**
             * Format: int32
             * @description 运行着这个构建的实例数
             */
            instances: number;
        };
        /** @description 一条订阅路由 */
        RouteItem: {
            /** @description 消息名称 */
            messageType: string;
            /** @description 订阅名 */
            subscription: string;
            /** @description 订阅方模块 */
            module: string;
            /** @description 状态 */
            status: components["schemas"]["MessageRouteStatus"];
            /** @description 第一次登记它的构建号 */
            registeredBuild: string;
            /**
             * Format: date-time
             * @description 第一次登记的时间
             */
            registeredAt: string;
            /** @description 是否还需要初始化 */
            needsInitialization: boolean;
            /** @description 显示名 */
            displayName: null | string;
            /** @description 退役原因，待终结和已终结的才有 */
            retiredReason: null | string;
            /**
             * Format: date-time
             * @description 终结的时间
             */
            finalizedAt: string;
            /**
             * Format: uuid
             * @description 终结的人
             */
            finalizedBy: null | string;
            /** @description 现在还在运行的实例里，哪些构建声明了它：判断能不能终结要看这个 */
            declaredByBuilds: components["schemas"]["RouteBuild"][];
        };
        /** @description 定时任务列表里的一项：代码里的默认值、运维改过的覆盖值、实际生效的状态和最近一次执行 */
        ScheduledJobItem: {
            /** @description 稳定名称 */
            name: string;
            /** @description 所属模块；框架自带的任务是 Framework */
            module: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 代码里还有没有这个任务；已经删除的只剩覆盖设置，回滚后配置仍在 */
            isDeclared: boolean;
            /** @description 代码里的默认执行时间；已经删除的任务没有 */
            defaultCron: null | string;
            /** @description 运维设的开关；空表示没改，跟着默认值（启用） */
            overrideEnabled: null | boolean;
            /** @description 运维设的执行时间；空表示没改，跟着默认值 */
            overrideCron: null | string;
            /**
             * Format: int64
             * @description 覆盖设置的版本号，改它时带回来；从来没改过的任务没有
             */
            overrideVersion: null | number;
            /** @description 按默认值和覆盖值，应该是启用还是停用 */
            enabled: boolean;
            /** @description 按默认值和覆盖值，应该用的执行时间 */
            cron: null | string;
            /** @description 调度器里触发器现在用的执行时间；停用或还没同步时为空 */
            activeCron: null | string;
            /**
             * Format: date-time
             * @description 下一次触发的时刻；停用时为空
             */
            nextFireTime: string;
            /** @description 调度器里的状态和“应该”是否一致；改了设置却同步失败时为否 */
            inSync: boolean;
            lastRun: null | components["schemas"]["ScheduledJobRunItem"];
            /** @description 当前用户能对它做的动作：Edit、RestoreDefault、RunNow */
            allowedActions: string[];
        };
        /** @description 定时任务的一次执行 */
        ScheduledJobRunItem: {
            /**
             * Format: uuid
             * @description 记录 ID
             */
            id: string;
            /** @description 任务名 */
            jobName: string;
            /**
             * Format: date-time
             * @description 计划触发的时刻
             */
            scheduledFireTime: string;
            /**
             * Format: date-time
             * @description 开始执行的时刻
             */
            startedAt: string;
            /**
             * Format: date-time
             * @description 结束的时刻；还在执行时为空
             */
            finishedAt: string;
            /**
             * Format: int64
             * @description 耗时，毫秒
             */
            durationMs: null | number;
            /** @description 怎么触发的：按计划、手动、补执行 */
            trigger: components["schemas"]["JobRunTrigger"];
            /**
             * Format: uuid
             * @description 手动执行的人
             */
            triggeredBy: null | string;
            /** @description 手动执行的人的名称；用户已经不存在时为空 */
            triggeredByName: null | string;
            /** @description 状态 */
            status: components["schemas"]["JobRunStatus"];
            /**
             * Format: int32
             * @description 处理了多少条
             */
            itemsProcessed: null | number;
            /** @description 命令给的简短说明 */
            note: null | string;
            /** @description 失败时的错误码 */
            errorCode: null | string;
            /** @description 失败时的错误说明 */
            errorMessage: null | string;
            /** @description 执行它的调度器实例 */
            instanceId: string;
            /** @description 链路追踪 ID，可以到日志与链路里查这次执行 */
            traceId: null | string;
        };
        /** @description 安全事件列表里的一行 */
        SecurityEventItem: {
            /**
             * Format: uuid
             * @description 事件 ID
             */
            id: string;
            /**
             * Format: date-time
             * @description 发生时间
             */
            occurredAt: string;
            /** @description 类型 */
            type: string;
            /** @description 严重程度 */
            severity: components["schemas"]["SecuritySeverity"];
            /** @description 原因编码 */
            reason: null | string;
            /**
             * Format: uuid
             * @description 用户
             */
            userId: null | string;
            /**
             * Format: uuid
             * @description 模拟身份时实际操作的人
             */
            actualUserId: null | string;
            /**
             * Format: uuid
             * @description 当时的角色
             */
            roleId: null | string;
            /**
             * Format: uuid
             * @description 会话
             */
            sessionId: null | string;
            /** @description 登录失败时输入的账号 */
            attemptedAccount: null | string;
            /** @description 客户端 IP */
            clientIp: null | string;
            /** @description 资源类型 */
            resourceType: null | string;
            /**
             * Format: uuid
             * @description 资源 ID
             */
            resourceId: null | string;
            /** @description 补充说明 */
            details: null | {
                [key: string]: string;
            };
            /** @description 链路追踪 ID，可以到日志与链路里查这次请求 */
            traceId: string;
        };
        /**
         * @description 严重程度
         * @enum {unknown}
         */
        SecuritySeverity: "Info" | "Warning" | "High";
        /** @description 替换授予上锚点的请求 */
        SetGrantAnchorsRequest: {
            /**
             * Format: int64
             * @description 调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的锚点，必须是启用的组织单元；给空列表表示不设锚点，从用户所属的组织单元算 */
            anchors: unknown[];
        };
        /** @description 改定时任务开关和执行时间的请求：整体替换，空的项恢复成跟着默认值 */
        SetJobOverrideRequest: {
            /** @description 是否启用；空表示跟着默认值（启用） */
            enabled: null | boolean;
            /** @description 执行时间的 Cron 表达式，Quartz 写法（带秒），按业务时区解释，两次执行至少隔 1 分钟；空表示跟着代码里的默认值 */
            cron: null | string;
            /**
             * Format: int64
             * @description 调用方看到的版本号（列表里的 overrideVersion）；这个任务还没改过时不给。别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 保存本人偏好的请求 */
        SetMyPreferenceRequest: {
            /** @description 值，JSON 对象；目录里声明的偏好按它的成员读出并校验，偏好族里的不理解内容 */
            value: unknown;
        };
        /** @description 设置偏好在系统层或角色层的值 */
        SetPreferenceLayerRequest: {
            /** @description 值，JSON 对象；目录里声明的偏好按它的成员读出并校验，偏好族里的不理解内容 */
            value: unknown;
            /**
             * Format: int64
             * @description 调用方看到的版本号；这一层还没有值时不给。别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 替换角色数据范围的请求 */
        SetRoleDataScopesRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 默认等级 */
            defaultLevel: components["schemas"]["RoleScopeLevel"];
            /** @description 按范围对象单独设置的等级，键是范围对象编码 */
            overrides: Record<string, never>;
        };
        /** @description 替换角色权限点的请求 */
        SetRolePermissionsRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的权限点集合，只接受目录里存在的编码 */
            permissions: string[];
        };
        /** @description 修改系统设置的请求 */
        SetSettingRequest: {
            /** @description 新的值，JSON 对象；成员见设置里的字段清单，通不过校验时错误按成员名返回 */
            value: unknown;
            /** @description 原因，必填，最多 500 个字符，记入变更历史 */
            reason: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号；设置还在用默认值时不给。别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 设置变更历史的一行 */
        SettingChangeItem: {
            /**
             * Format: uuid
             * @description 记录 ID
             */
            id: string;
            /** @description 设置键 */
            key: string;
            /** @description 改之前的值；此前用的是默认值时为空 */
            oldValue: unknown;
            /** @description 改之后的值；恢复默认或清除时为空 */
            newValue: unknown;
            /** @description 原因 */
            reason: string;
            /**
             * Format: uuid
             * @description 操作人
             */
            changedBy: string;
            /** @description 操作人的姓名 */
            changedByName: null | string;
            /**
             * Format: date-time
             * @description 时间
             */
            changedAt: string;
            /** @description 链路追踪 ID */
            traceId: null | string;
        };
        /** @description 设置值里的一个成员：管理页据此生成编辑控件 */
        SettingFieldInfo: {
            /** @description 成员名，与 JSON 里的字段名一致 */
            name: string;
            /** @description 类型：bool、int、long、decimal、double、string、guid、date、time、dateTime、timeSpan */
            type: string;
            /** @description 显示名；没有声明时是成员名 */
            displayName: string;
            /** @description 单位 */
            unit: null | string;
            /** @description 是否可空 */
            isNullable: boolean;
            /** @description 是否数组 */
            isArray: boolean;
        };
        /** @description 一个系统设置：定义、默认值、当前值，以及当前用户能对它做什么 */
        SettingItem: {
            /** @description 设置键 */
            key: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            module: string;
            /** @description 分组 */
            group: null | string;
            /** @description 值里的成员清单，管理页据此生成编辑控件 */
            fields: components["schemas"]["SettingFieldInfo"][];
            /** @description 代码里的默认值 */
            defaultValue: unknown;
            /** @description 当前生效的值：被修改过就是修改后的，否则是默认值 */
            currentValue: unknown;
            /** @description 是否被修改过 */
            isModified: boolean;
            /**
             * Format: int64
             * @description 值行的版本号，输出为 version；还在用默认值时为空
             */
            version: null | number;
            /** @description 修改它需要的权限点 */
            editPermission: string;
            /** @description 当前用户现在可以对它执行的动作 */
            allowedActions: string[];
        };
        /** @description 恢复默认、清除设置值的请求 */
        SettingReasonRequest: {
            /** @description 原因，必填，最多 500 个字符，记入变更历史 */
            reason: string;
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 全部系统设置 */
        SettingsResponse: {
            /** @description 代码里声明的设置，按模块、分组、键排好 */
            settings: components["schemas"]["SettingItem"][];
            /** @description 代码里已经不存在、值行还留着的设置 */
            obsolete: components["schemas"]["ObsoleteSettingItem"][];
        };
        /** @description 站内消息列表里的一条 */
        SiteMessageItem: {
            /**
             * Format: uuid
             * @description 消息
             */
            id: string;
            /** @description 标题 */
            title: string;
            /** @description 正文，前端显示时转义 */
            body: string;
            /** @description 点击跳转的站内地址 */
            link: null | string;
            /** @description 类别 */
            category: string;
            /**
             * Format: date-time
             * @description 发出的时间
             */
            sentAt: string;
            /** @description 是否已读 */
            isRead: boolean;
            /**
             * Format: date-time
             * @description 已读的时间
             */
            readAt: string;
        };
        /** @description 标本附件，用关联时保存的快照显示，不查 Files */
        SpecimenAttachmentResponse: {
            /**
             * Format: uuid
             * @description 附件
             */
            id: string;
            /** @description 文件名 */
            name: string;
            /**
             * Format: int64
             * @description 字节数
             */
            size: number;
            /** @description 文件类型编码 */
            fileType: string;
        };
        /** @description 示例标本的详情 */
        SpecimenDetailResponse: {
            /**
             * Format: uuid
             * @description 标本
             */
            id: string;
            /**
             * Format: int64
             * @description 并发版本号，修改时作为 expectedVersion 带回
             */
            version: number;
            /** @description 编号 */
            number: string;
            /** @description 标题 */
            title: string;
            /** @description 状态 */
            status: components["schemas"]["SpecimenStatus"];
            /** @description 明细 */
            items: components["schemas"]["SpecimenItemResponse"][];
            /** @description 附件 */
            attachments: components["schemas"]["SpecimenAttachmentResponse"][];
            /** @description 当前用户现在可以执行的动作，如 Submit、Close */
            allowedActions: string[];
        };
        /** @description 标本明细 */
        SpecimenItemResponse: {
            /** @description 名称 */
            name: string;
            /**
             * Format: int32
             * @description 数量
             */
            quantity: number;
        };
        /** @description 列表里的一行示例标本 */
        SpecimenListItem: {
            /**
             * Format: uuid
             * @description 标本
             */
            id: string;
            /** @description 编号 */
            number: string;
            /** @description 标题 */
            title: string;
            /** @description 状态 */
            status: components["schemas"]["SpecimenStatus"];
            /**
             * Format: date-time
             * @description 创建时间
             */
            createdAt: string;
        };
        /**
         * @description 示例标本的状态
         * @enum {string}
         */
        SpecimenStatus: "Draft" | "Submitted" | "Closed";
        /** @description 开始模拟身份的请求 */
        StartImpersonationRequest: {
            /**
             * Format: uuid
             * @description 以他的哪个角色模拟；不给就取他上次使用的角色，没有时取排在最前的启用角色
             */
            roleId: string;
            /** @description 原因，必填，最多 500 个字符，写入安全事件 */
            reason: string;
        };
        /** @description 业务历史记录 */
        StateHistoryItem: {
            /** @description 状态名 */
            stateName: string;
            /** @description 原状态编码 */
            fromCode: null | string;
            /** @description 新状态编码 */
            toCode: string;
            /** @description 动作编码 */
            actionCode: string;
            /** @description 原因 */
            reason: null | string;
            /**
             * Format: int32
             * @description 业务版本号
             */
            revision: null | number;
            actor: null | components["schemas"]["ReferenceInfo"];
            actualUser: null | components["schemas"]["ReferenceInfo"];
            role: null | components["schemas"]["ReferenceInfo"];
            onBehalfOf: null | components["schemas"]["ReferenceInfo"];
            /**
             * Format: date-time
             * @description 发生时间
             */
            occurredAt: string;
        };
        /** @description 存储问题列表里的一行 */
        StorageIssueItem: {
            /**
             * Format: uuid
             * @description 存储问题
             */
            id: string;
            /**
             * Format: uuid
             * @description 出问题的文件
             */
            fileId: string;
            /** @description 文件名，文件的记录已经清理时为空 */
            fileName: null | string;
            /** @description 种类 */
            kind: components["schemas"]["StorageIssueKind"];
            /** @description 发现时文件记录的对象版本，还原时按它找 */
            objectVersionId: null | string;
            /**
             * Format: date-time
             * @description 发现的时间
             */
            foundAt: string;
            /** @description 状态 */
            status: components["schemas"]["StorageIssueStatus"];
            /**
             * Format: date-time
             * @description 标为已解决的时间
             */
            resolvedAt: string;
            /**
             * Format: uuid
             * @description 标为已解决的人
             */
            resolvedBy: string;
            /** @description 处理说明 */
            resolutionNote: null | string;
            /**
             * Format: int64
             * @description 并发版本号
             */
            version: number;
        };
        /**
         * @description 存储问题的种类
         * @enum {string}
         */
        StorageIssueKind: "RestorableMissing" | "LostMissing";
        /**
         * @description 存储问题的状态
         * @enum {string}
         */
        StorageIssueStatus: "Open" | "Resolved";
        /**
         * @description 文件的状态
         * @enum {string}
         */
        StoredFileStatus: "Uploading" | "Scanning" | "Available" | "Attached" | "Detached" | "Rejected" | "Deleting" | "Purged";
        /** @description 提交示例标本的请求 */
        SubmitSpecimenRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 一个订阅在收件箱里的积压 */
        SubscriptionBacklogItem: {
            /** @description 订阅名 */
            subscription: string;
            /**
             * Format: int64
             * @description 还没处理完的条数
             */
            pending: number;
            /**
             * Format: int64
             * @description 最老的一条收到多少秒了；没有积压时为 0
             */
            oldestAgeSeconds: number;
            /**
             * Format: int64
             * @description 死信条数
             */
            dead: number;
            /**
             * Format: int64
             * @description 运维放弃的条数
             */
            abandoned: number;
            /**
             * Format: int64
             * @description 被死信挡住的顺序键个数，只有有序订阅才有
             */
            blockedKeys: number;
            /** @description 是不是待终结的订阅：积压是预期内的，不计入通用的积压告警 */
            isRetiring: boolean;
        };
        /** @description 切换角色的请求 */
        SwitchRoleRequest: {
            /**
             * Format: uuid
             * @description 要切换到的角色
             */
            roleId: string;
        };
        /**
         * @description 模板的渠道：模板可以发往哪些渠道，每个渠道的内容单独发布版本
         * @enum {string}
         */
        TemplateChannel: "Email" | "Sms" | "InApp";
        /** @description 模板在一个渠道当前的内容 */
        TemplateChannelInfo: {
            /** @description 渠道编码：Email、Sms、InApp */
            channel: string;
            /**
             * Format: int32
             * @description 生效的发布版本号，用代码里的默认内容时为空
             */
            activeVersion: null | number;
            /** @description 当前的主题；站内消息里是标题 */
            subject: string;
            /** @description 当前的正文 */
            body: string;
            /** @description 代码里的默认主题；代码里已经删掉的模板没有 */
            defaultSubject: null | string;
            /** @description 代码里的默认正文；代码里已经删掉的模板没有 */
            defaultBody: null | string;
        };
        /** @description 一个通知模板的详情 */
        TemplateDetailResponse: {
            /** @description 模板键 */
            key: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            module: string;
            /** @description 发件账号名，只对邮件渠道有意义 */
            senderProfile: string;
            /** @description 内容含一次性短时凭据 */
            carriesSecret: boolean;
            /** @description 状态编码：Active、Retired */
            status: string;
            /**
             * Format: int64
             * @description 并发版本号，发布和恢复时作为期望版本传回，输出为 version
             */
            version: number;
            /** @description 各渠道当前的内容 */
            channels: components["schemas"]["TemplateChannelInfo"][];
            /** @description 模板里能用的变量名 */
            variables: string[];
            /** @description 示例数据，预览默认用它；代码里已经删掉的模板没有 */
            example: unknown;
            /** @description 发布过的版本，新的在前 */
            versions: components["schemas"]["TemplateVersionInfo"][];
        };
        /** @description 列表里的一个模板 */
        TemplateListItem: {
            /** @description 模板键 */
            key: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            module: string;
            /** @description 发往的渠道编码 */
            channels: string[];
            /** @description 内容含一次性短时凭据 */
            carriesSecret: boolean;
            /** @description 状态编码：Active、Retired */
            status: string;
            /** @description 各渠道生效的版本号，没有的渠道用代码里的默认内容 */
            activeVersions: {
                [key: string]: number;
            };
            /**
             * Format: date-time
             * @description 最近一次发布的时间，没发布过的为空
             */
            lastPublishedAt: string;
        };
        /** @description 模板列表，按模板键排序 */
        TemplateListResponse: {
            /** @description 模板 */
            items: components["schemas"]["TemplateListItem"][];
        };
        /** @description 预览的结果 */
        TemplatePreviewResponse: {
            /** @description 渲染后的主题；模板有误时为空 */
            subject: null | string;
            /** @description 渲染后的正文；模板有误时为空 */
            body: null | string;
            /** @description 模板有误时的原因，包含出错的位置；没问题时为空 */
            error: null | string;
        };
        /** @description 发布过的一个版本 */
        TemplateVersionInfo: {
            /**
             * Format: int32
             * @description 版本号
             */
            version: number;
            /** @description 渠道编码 */
            channel: string;
            /** @description 主题 */
            subject: string;
            /** @description 正文 */
            body: string;
            /**
             * Format: uuid
             * @description 发布人
             */
            publishedBy: string;
            /**
             * Format: date-time
             * @description 发布时间
             */
            publishedAt: string;
            /** @description 备注 */
            remark: null | string;
        };
        /** @description 未读的站内消息数 */
        UnreadCountResponse: {
            /**
             * Format: int32
             * @description 条数
             */
            count: number;
        };
        /** @description 修改组织单元的请求 */
        UpdateOrgUnitRequest: {
            /**
             * Format: int64
             * @description 调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 名称，最多 100 个字符，同一上级下不区分大小写唯一 */
            name: string;
            /** @description 编码，最多 50 个字符；不填表示清空 */
            code: null | string;
        };
        /** @description 修改角色的请求 */
        UpdateRoleRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 名称 */
            name: string;
            /** @description 说明 */
            description: null | string;
            /**
             * Format: int32
             * @description 次序
             */
            sortOrder: number;
        };
        /** @description 修改用户资料的请求 */
        UpdateUserProfileRequest: {
            /**
             * Format: int64
             * @description 调用方看到的版本号，别人改过就返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 登录名；改它与改手机号一样，对持有受保护角色的用户只有同样持有受保护角色的人才能做 */
            account: string;
            /** @description 姓名 */
            displayName: string;
            /** @description 英文名 */
            englishName: null | string;
            /** @description 工号 */
            employeeNo: null | string;
            /** @description 手机号 */
            mobile: null | string;
            /** @description 邮箱 */
            email: null | string;
        };
        /** @description 登记好的上传：往哪里写、写完登记到哪个文件 */
        UploadStarted: {
            /**
             * Format: uuid
             * @description 文件 ID
             */
            fileId: string;
            /** @description 写入存储用的键 */
            storageKey: string;
            /** @description 整理后的文件名 */
            fileName: string;
        };
        /** @description 授予上的一个锚点 */
        UserAnchorItem: {
            /**
             * Format: uuid
             * @description 组织单元 ID
             */
            orgUnitId: string;
            /** @description 组织单元的名称 */
            name: null | string;
        };
        /** @description 用户详情 */
        UserDetailResponse: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 版本号，输出为 version
             */
            version: number;
            /** @description 登录名 */
            account: string;
            /** @description 姓名 */
            displayName: string;
            /** @description 英文名 */
            englishName: null | string;
            /** @description 工号 */
            employeeNo: null | string;
            /** @description 手机号 */
            mobile: null | string;
            /** @description 邮箱 */
            email: null | string;
            /** @description 状态 */
            status: components["schemas"]["UserStatus"];
            /**
             * Format: uuid
             * @description 所属组织单元
             */
            orgUnitId: string;
            /** @description 所属组织单元的名称 */
            orgUnitName: null | string;
            /** @description 是否设置过密码；迁移来的员工可能还没有，靠短信登录 */
            hasPassword: boolean;
            /** @description 下次登录是否必须先改密码 */
            passwordChangeRequired: boolean;
            /**
             * Format: date-time
             * @description 密码最近一次变化的时间
             */
            passwordChangedAt: string;
            /**
             * Format: date-time
             * @description 登录被限制到什么时候；没有被限制时为空。只影响新的登录尝试，不影响已有会话
             */
            loginLockedUntil: string;
            /** @description 角色授予 */
            grants: components["schemas"]["UserGrantItem"][];
            /**
             * Format: int32
             * @description 仍然有效的会话数，含他发起的模拟会话
             */
            activeSessionCount: number;
            /** @description 当前用户现在可以对他执行的动作 */
            allowedActions: string[];
        };
        /** @description 用户详情里的一项角色授予 */
        UserGrantItem: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            roleId: string;
            /** @description 角色名称 */
            roleName: null | string;
            /** @description 角色是否启用；停用的角色用户登录后用不了 */
            roleIsActive: boolean;
            /** @description 是否受保护角色 */
            roleIsProtected: boolean;
            /**
             * Format: date-time
             * @description 授予时间
             */
            grantedAt: string;
            /**
             * Format: uuid
             * @description 授予人
             */
            grantedBy: string;
            /** @description 授予人的姓名 */
            grantedByName: null | string;
            /** @description 数据范围的锚点：这个角色的“本组织”从哪些组织单元算起 */
            anchors: components["schemas"]["UserAnchorItem"][];
        };
        /** @description 用户列表的一行 */
        UserListItem: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            id: string;
            /** @description 登录名 */
            account: string;
            /** @description 姓名 */
            displayName: string;
            /** @description 工号 */
            employeeNo: null | string;
            /** @description 手机号 */
            mobile: null | string;
            /** @description 状态 */
            status: components["schemas"]["UserStatus"];
            /**
             * Format: uuid
             * @description 所属组织单元
             */
            orgUnitId: string;
            /** @description 所属组织单元的名称 */
            orgUnitName: null | string;
            /** @description 拥有的角色 */
            roles: components["schemas"]["UserRoleBrief"][];
            /**
             * Format: int64
             * @description 版本号，输出为 version
             */
            version: number;
        };
        /** @description 选人下拉里的一项 */
        UserOption: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            id: string;
            /** @description 姓名 */
            displayName: string;
            /** @description 工号 */
            employeeNo: null | string;
            /** @description 所属组织单元的名称 */
            orgUnitName: null | string;
            /** @description 状态；已禁用的用户界面上标出来 */
            status: components["schemas"]["UserStatus"];
        };
        /** @description 用户列表里一个角色的简要信息 */
        UserRoleBrief: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            id: string;
            /** @description 名称 */
            name: string;
            /** @description 角色是否启用；停用的角色用户登录后用不了 */
            isActive: boolean;
        };
        /** @description 用户的一个有效会话 */
        UserSessionItem: {
            /**
             * Format: uuid
             * @description 会话 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 会话属于哪个用户；模拟会话属于被模拟的人
             */
            userId: string;
            /**
             * Format: uuid
             * @description 模拟会话里实际操作的人；普通会话为空
             */
            impersonatorUserId: string;
            /** @description 客户端类型 */
            clientType: components["schemas"]["ClientType"];
            /** @description 设备名，取自登录时的 User-Agent */
            deviceName: null | string;
            /** @description 登录时的 IP */
            loginIp: null | string;
            /**
             * Format: date-time
             * @description 登录时间
             */
            createdAt: string;
            /**
             * Format: date-time
             * @description 最近一次刷新令牌的时间
             */
            lastRefreshedAt: string;
            /**
             * Format: date-time
             * @description 不再刷新就在什么时候失效
             */
            refreshExpiresAt: string;
        };
        /**
         * @description 用户的状态
         * @enum {string}
         */
        UserStatus: "Active" | "Disabled";
        /** @description 修改结果 */
        VersionedResponseOfDictionaryTypeId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfNotificationTemplateId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfOrgTreeId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfPreferenceValueId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfRoleId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfScheduledJobSettingId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfSpecimenId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfStorageIssueId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfStoredFileId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfSystemSettingValueId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
        /** @description 修改结果 */
        VersionedResponseOfUserId: {
            /**
             * Format: uuid
             * @description ID
             */
            id: string;
            /**
             * Format: int64
             * @description 保存后的版本号，输出为 version
             * @default 0
             */
            version: number;
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export type operations = Record<string, never>;
