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
         * 查询捕获内容详情
         * @description 返回捕获内容及正文。未开启捕获或记录不存在时返回 404。内容可能含验证码，查看会记录读审计。
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
         * @description 查询捕获模式下保存的邮件和短信，不含正文。未开启捕获时返回 404。内容可能含验证码，查看会记录读审计。
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
                    /** @description 排序，如 -capturedAt：字段名前加 - 表示倒序，默认按捕获时间倒序 */
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
         * @description 仅可放弃失败且未放弃的投递，否则返回 422。放弃后保留失败状态并记录原因和时间；之后不可重试，可按保留期清理。成功返回 204。必须提供幂等键。
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
         * 查询投递详情
         * @description 返回渲染后的主题和正文。含密投递发送成功、最终失败或跳过后清空正文。查看会记录读取审计。
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
         * @description 分页返回投递状态、脱敏地址、领取次数和失败原因，不含正文。每条记录对应一个收件人和一个渠道，可按收件用户、渠道、状态、模板和创建时间筛选。
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
                    /** @description 创建时间下限（含） */
                    From?: string;
                    /** @description 创建时间上限（不含） */
                    To?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 -createdAt：逗号分隔，字段名前加 - 表示倒序，默认按创建时间倒序 */
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
         * @description 仅可重试失败且未放弃的投递，否则返回 422；含密通知失败时内容已清除，一律不能重试。重试后重新排队，领取次数清零。成功返回 204。必须提供幂等键。
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
         * @description 返回本人的未读站内消息数，供前端轮询。
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
         * @description 分页查询本人的站内消息，默认未读在前、发送时间倒序。支持按已读状态和类别筛选。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 是否未读：true 为未读，false 为已读；不传时返回全部 */
                    Unread?: boolean;
                    /** @description 消息类别筛选 */
                    Category?: string;
                    /** @description 页码，从 1 开始 */
                    Page?: number;
                    /** @description 每页数量，1 到 200，默认 20 */
                    PageSize?: number;
                    /** @description 排序，如 isRead,-sentAt：逗号分隔，字段名前加 - 表示倒序，默认未读在前，再按发送时间倒序 */
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
         * 将我的全部未读站内消息标为已读
         * @description 返回本次标记为已读的消息数。
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
         * 将我的指定站内消息标为已读
         * @description 只能标记当前用户的消息。消息不存在或属于其他用户时返回 404；成功返回 204。消息已读时保留原已读时间。
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
         * 查询通知模板详情
         * @description 返回各渠道内容、代码默认内容、可用变量、示例数据及已发布版本。已移除代码声明的模板不提供默认内容、变量和示例数据。发布或恢复默认内容时，须将响应中的 version 作为预期版本号提交。
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
         * @description 返回模板渠道、状态、各渠道选用的发布版本及最近发布时间。活动模板未选用发布版本的渠道使用代码默认内容。
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
         * @description 仅预览，不保存，渲染限制与正式发送相同；预览还会检查本次遇到的未定义变量。渲染错误仍返回 200，原因见 error 字段；模板不存在、渠道或数据校验失败时返回对应错误。
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
         * 发布指定渠道的模板新版本
         * @description 发布后，后续渲染使用新版本，已生成的投递内容不变。发布前用示例数据严格渲染，语法错误、遇到未定义变量或超限时返回 422；未执行的条件分支不在校验范围。已退役的模板不能发布。必须带幂等键。
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
         * @description 恢复指定渠道的代码默认内容，保留已发布的历史版本。必须提供幂等键。
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
         * @description 返回文件元数据、状态、引用、状态历史及彻底删除记录，不提供存储键或下载地址。管理员下载也须通过业务对象授权。
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
         * @description 分页查询文件元数据和状态，不含内容或存储键。支持按文件名、状态、上传配置、上传人或生成请求人、登记时间及引用文件的业务对象筛选。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 文件名搜索关键字 */
                    Q?: string;
                    /** @description 状态编码，多个值须重复参数名，如 status=Available&amp;status=Attached */
                    Status?: string[];
                    /** @description 上传配置标识；生成文件为 Generated:来源 */
                    Profile?: string;
                    /** @description 上传人或生成请求人 ID */
                    UploadedBy?: string;
                    /** @description 文件登记时间下限（含） */
                    From?: string;
                    /** @description 文件登记时间上限（不含） */
                    To?: string;
                    /** @description 按引用文件的业务模块筛选；未指定时忽略 ownerType 和 ownerId */
                    OwnerModule?: string;
                    /** @description 业务对象类型 */
                    OwnerType?: string;
                    /** @description 业务对象 ID */
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
         * 上传文件
         * @description 使用 multipart/form-data，仅处理第一个带文件名的部分。profile 指定上传配置，决定大小上限、允许类型和所需权限；按扩展名及文件头识别类型，不采用客户端声明的内容类型。成功后返回临时文件 ID，须在保留期内（默认 24 小时）关联业务对象，未关联的到期后可被清理。超过大小上限返回 413，类型不允许返回 415。
         */
        post: {
            parameters: {
                query: {
                    /** @description 上传配置名称，如 Sample.SpecimenAttachment */
                    profile: string;
                    /** @description 文件大小（字节），可选；声明大小超过配置上限时直接拒绝，实际读取仍受该上限限制 */
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
         * @description 分页查询对象缺失的存储问题。“可以还原”表示发现问题时仍能读取记录中的对象版本；运维还原后可登记处理结果。
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
         * @description 彻底删除后无法读取内容，操作不可恢复，引用方显示“文件已删除”。删除原因必填，历史版本须由运维另行清除。版本校验通过后，文件已彻底删除或正在删除时重复操作直接成功。存在并发关联或未完成的关联请求时返回 503，可稍后重试。
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
         * 登记文件历史版本清除记录
         * @description 运维清除对象存储中的文件历史版本后，登记执行记录（如工单号）。仅已彻底删除的文件可登记。
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
         * @description 登记存储问题的处理结果，不执行对象还原。处理说明必填。版本校验通过后，已解决的问题仍返回成功，并保留原处理人、时间和说明。
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
         * 重试删除文件
         * @description 删除尝试达到上限后停止自动重试。排除存储故障后，可重置删除队列中文件的尝试次数。文件不存在或不在删除队列中返回 404，成功返回 204。
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
         * @description 须有审计导出权限；下载他人的导出还须有管理作业权限。作业不存在、类型不符或无权查看该作业时返回 404。作业未完成、或刚完成而文件还在登记时，返回可重试的 409；失败或取消返回 422。文件不存在时返回 404。下载会记录读审计。
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
         * @description 将指定时间段的审计记录导出为 CSV，筛选条件与审计列表相同。提交后返回 202 和 jobId，Location 指向下载地址；作业未完成时下载返回 409，可稍后重试。行数超限时作业失败，请缩小时间范围后重新导出。
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
         * 查询审计记录详情
         * @description 包含字段变更和补充说明。操作人、角色名称实时查询，不存在时返回 null。查询本身也记录读取审计。
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
         * @description 时间范围必填，最长 93 天。按发生时间倒序，使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。不按数据范围过滤，审计人员可查看全部记录；查询本身也记录读取审计。
         */
        get: {
            parameters: {
                query: {
                    /** @description 起始时间，含；必填 */
                    From: string;
                    /** @description 结束时间，不含；必填，和起始时间相隔不超过 93 天 */
                    To: string;
                    /** @description 操作人 ID */
                    ActorUserId?: string;
                    /** @description 模拟身份时的实际操作人 ID */
                    ActualUserId?: string;
                    /** @description 产生审计的模块，如 Identity */
                    Module?: string;
                    /** @description 操作名，如 Identity.CreateUser */
                    Action?: string;
                    /** @description 操作对象的类型，如 User */
                    ResourceType?: string;
                    /** @description 操作对象的 ID */
                    ResourceId?: string;
                    /** @description 上一页返回的游标，首页不传 */
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
         * 请求取消后台作业
         * @description 排队中的作业直接取消；运行中的先标记为取消中，执行器续期检测到后发出取消信号，处理器响应后停止。已提交的批次不回滚。作业已结束或正在取消时返回 409 Operations.BackgroundJobFinished。必须提供幂等键。
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
         * 请求取消本人提交的后台作业
         * @description 排队中的作业直接取消；运行中的先标记为取消中，执行器续期检测到后发出取消信号，处理器响应后停止。已提交的批次不回滚。作业不存在或由其他用户提交时均返回 404；作业已结束或正在取消时返回 409 Operations.BackgroundJobFinished。必须提供幂等键。
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
         * 查询本人提交的后台作业详情
         * @description 登录后可查询当前用户提交的作业。建议每 2 秒轮询一次，作业结束后停止。作业不存在或由其他用户提交时，均返回 404 Operations.BackgroundJobNotFound。
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
         * @description 汇总各模块的作业，按提交时间倒序返回状态、进度、结果摘要和错误，不返回作业参数。使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 模块筛选，未指定时查询全部模块 */
                    Module?: string;
                    /** @description 作业类型名，如 Sample.ExportSpecimens */
                    JobType?: string;
                    /** @description 状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled */
                    Status?: string;
                    /** @description 提交人 */
                    SubmittedBy?: string;
                    /** @description 上一页返回的游标，首页不传 */
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
         * 查询本人提交的后台作业
         * @description 登录后可查询本人在各模块提交的作业，供页面中的作业入口使用。按提交时间倒序，使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled */
                    Status?: string;
                    /** @description 上一页返回的游标，首页不传 */
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
         * 批量上报前端错误
         * @description 登录后可上报，包括必须修改密码的账号。仅写入 Kmic.ClientErrors 日志，不存入数据库，无需幂等键。默认每批最多 20 条；请求按实例、按用户分别限流，默认每分钟最多 30 次。超限返回 429，前端应丢弃该批次，不重试。仅接收规定字段；上报内容不得包含请求体、表单值或 URL 查询参数。
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
         * 放弃收件箱死信
         * @description 支持单条或批量操作，最多 200 条，原因必填。放弃后消息不再处理。接收方的放弃检查未通过时，保持原状态并在逐条结果中说明原因，只能重试。仅支持收件箱死信。必须提供幂等键。
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
         * @description 放弃等待确认的重建并清除当前初始化标记。如需再次重建，请手动触发。没有等待确认的重建时返回 409；成功返回 204。必须提供幂等键。
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
         * @description 运维核实缺失行在源端确实不存在后，确认等待收尾的重建。系统另排作业，将这些行置为终态并完成重建。没有等待确认的重建时返回 409；成功返回 204。必须提供幂等键。
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
         * @description 订阅须处于“待终结”状态。操作须在维护窗口执行；终结后停止投递，旧构建不能自动恢复订阅。仍有心跳未过期的实例声明该订阅，或消息处于“处理中”且占用未到期时，不能终结。未完成的消息须先处理完成，或在终结时选择放弃；放弃原因与原错误一并记录。必须提供幂等键。
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
         * 查看死信详情
         * @description 返回原始消息内容和信封信息，也可查看已放弃的消息。内容可能含敏感信息，查看会记录读审计。
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
         * 查询各模块的消息积压
         * @description 返回各模块发件箱的积压数、最早积压消息的等待时长和死信数，以及收件箱各订阅的积压、死信和阻塞顺序键数量。isRetiring 标记待终结订阅，其积压属预期情况，应与普通订阅分开告警。
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
         * 查询被死信阻塞的顺序键
         * @description 同一顺序键下的后续消息须等待最早的死信处理完成；可重试该死信；允许放弃且确认无需再处理时，也可放弃。每个模块最多返回 500 个键，按等待消息数倒序排列。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 模块筛选，未指定时查询全部模块 */
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
         * @description 汇总各模块收件箱和发件箱的死信，按发生时间倒序返回。使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。列表不含消息内容，需通过详情接口查看。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 模块筛选，未指定时查询全部模块 */
                    Module?: string;
                    /** @description Outbox 或 Inbox */
                    Box?: string;
                    /** @description 订阅名，指定后仅查询收件箱 */
                    Subscription?: string;
                    /** @description 消息名称 */
                    MessageType?: string;
                    /** @description 上一页返回的游标，首页不传 */
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
         * 查询消息处理实例
         * @description 列出实例的构建号、角色、心跳和订阅，按最近心跳倒序。包含心跳已过期的记录，供诊断订阅声明情况。
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
         * @description 从声明了读模型重建的模块中各取最近 20 条记录，合并后按开始时间倒序返回。等待确认表示缺失行数超限，须由运维核实后确认收尾或放弃。
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
         * @description 返回消息类型、订阅方模块、订阅名及路由状态，并列出心跳未过期且声明该订阅的构建，供终结前核对。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 状态编码，多值须重复参数名；未指定时查询全部状态 */
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
         * @description 仅可重新激活已终结的订阅，恢复后继续投递。终结期间的消息不会自动补投，须核对缺口后补投。成功返回 204。必须提供幂等键。
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
         * @description 将来源模块发件箱中指定序号区间内、已投递的消息补投给指定订阅，已接收的消息不重复写入。单次最多 5000 条，超限请缩小区间。已终结的订阅须先重新激活。必须提供幂等键。
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
         * 重试死信
         * @description 支持单条或批量操作，最多 200 条。将死信恢复为待处理，重试次数清零并允许立即领取。逐条返回结果，不存在或状态不符的消息不影响其他消息。未解决的问题仍可能导致重试失败。必须提供幂等键。
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
         * @description 为已声明重建方式的订阅提交重建作业。已有未结束的重建任务时返回 409。成功返回 204。必须提供幂等键。
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
         * 查询定时任务执行记录
         * @description 按开始时间倒序，使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。任务声明已移除时，仍可查询历史记录。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 上一页返回的游标，首页不传 */
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
         * @description 列出代码声明的任务及其配置、调度状态和最近执行记录。已移除声明但仍有自定义设置的任务也会列出，isDeclared 为 false。
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
         * 校验 Cron 并预览后续触发时间
         * @description 使用业务时区和含秒的 Quartz Cron 格式，如 0 0 7 * * ?。格式无效、未来不再触发或预览中的触发间隔不足 1 分钟时，返回 cron 字段错误。
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
         * 恢复定时任务默认设置
         * @description 清空自定义启用状态和执行时间，保留设置记录供追溯，后续使用代码默认值。任务已移除时返回 404；设置记录不存在或版本不符时返回 409。必须提供幂等键。
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
         * 手动触发一次定时任务
         * @description 提交手动触发请求后返回，不等待任务完成；结果见执行记录，其中保留操作人。检查到任务正在运行时返回 409 Operations.JobAlreadyRunning，不提交触发请求；停用的任务也可手动执行。必须提供幂等键。
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
         * 修改定时任务的启用状态和执行时间
         * @description 整体替换自定义设置，null 表示使用代码默认值。保存后尝试同步至集群调度器，同步失败不影响保存。列表中的 inSync 表示实际调度状态是否与配置一致。必须提供幂等键。
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
         * @description 同一类型内的编码唯一，不区分大小写，创建后不可修改。非层级字典不能指定父项。必须提供幂等键。
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
         * @description 停用项仍供历史数据显示，新数据不可选用。有启用的子项时返回 422，须先停用子项。
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
         * @description 有父项时，父项必须已启用。
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
         * 批量查询字典
         * @description types 为逗号分隔的类型编码，每次最多 20 个。登录即可查询，结果包含停用项及状态，不存在的类型会被忽略。响应包含 ETag；携带 If-None-Match 再次请求时，版本未变则返回 304。
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
         * 查询字典类型及全部字典项
         * @description 供管理页使用，包含停用项和版本号。修改字典项时须提交返回的 version。
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
         * @description 返回字典类型、所属模块、字典项数量和状态。类型由代码声明，管理页仅维护字典项。
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
         * @description 新父项须已启用，且不能是自身或后代；非层级字典不能指定父项。不指定父项表示顶层，父项变更后排在同级末尾。
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
         * @description 仅修改显示名。业务数据通过编码引用字典项，因此编码不可修改。
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
         * 调整同级字典项顺序
         * @description 须提交同一父项下的全部字典项（含停用项），且不得重复。
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
         * @description 登录后可查询已开启的功能开关，供前端控制入口显示。开关来自部署配置，只决定功能是否开放，不授予权限。
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
         * 查询个人偏好
         * @description 返回当前角色下的生效偏好值及来源（默认、系统、角色或个人），包括已有可用值的偏好族键。前端启动时读取，修改后更新本地数据。
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
         * @description 供管理页使用，返回偏好定义及系统、角色层的设置值和版本，包括允许设置的层、代码默认值，以及已有系统或角色设置的偏好族键。不包含个人设置。
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
         * 重置个人偏好
         * @description 移除当前角色下的个人设置，按允许的角色层、系统层取有效值。均无可用值时，普通偏好使用代码默认值，个人偏好查询不再返回该偏好族键。未保存个人设置时不做修改；只读模拟会话不能操作。
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
         * 移除角色级偏好设置
         * @description 仅移除指定角色的设置。允许且有效的个人设置仍优先，否则取允许的系统层有效值。均无可用值时，普通偏好使用代码默认值，个人偏好查询不再返回该偏好族键。必须提供幂等键。
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
         * 移除系统级偏好设置
         * @description 移除系统层设置。个人层和角色层均无允许且有效的值时，普通偏好使用代码默认值，个人偏好查询不再返回该偏好族键。必须提供幂等键。
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
         * 保存个人偏好
         * @description 仅保存当前角色下的个人设置，再次保存同一键时覆盖原值。偏好族的键须符合格式要求，值须为大小符合限制的 JSON 对象，具体内容由前端约定。只读模拟会话不能保存。
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
         * 设置角色级偏好
         * @description 允许且有效的个人设置优先于角色设置。角色必须存在，偏好声明须允许角色层设置。必须提供幂等键。
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
         * 设置系统级偏好
         * @description 允许且有效的个人或角色设置优先于系统设置。偏好声明须允许系统层设置。必须提供幂等键。
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
        /** 查询系统设置详情 */
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
         * 修改系统设置
         * @description 需要查看设置权限及该设置声明的修改权限，可通过列表中的可用操作判断。值按声明类型解析并校验。必须提供幂等键。
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
         * @description 分页返回修改人、时间、原因及修改前后的值，默认按时间倒序。新旧值原样保存，设置中不得存放密钥。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 设置键筛选，未指定时查询全部设置 */
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
         * @description 返回已声明设置的字段、默认值、当前值、修改状态、修改权限和当前用户的可用操作，并单独列出已移除声明但仍保留值的设置。
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
         * 清除已移除声明的设置值
         * @description 仍有代码声明的设置不能清除，如需重置请恢复默认值。需要敏感权限“清除不再使用的设置值”。必须提供幂等键。
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
         * 恢复系统设置默认值
         * @description 移除自定义值，并记录新值为 null 的变更历史。权限要求与修改设置相同。必须提供幂等键。
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
         * @description 已有密码时须提供旧密码，包括管理员重置的临时密码；尚未设置密码时无需提供。成功后其他会话全部失效，当前会话保留。
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
         * 查询当前用户信息
         * @description 返回用户资料、当前角色及已获授的启用角色、当前权限、授权版本、密码修改要求、身份模拟状态和业务时区。当前角色不可用时仍可调用，供前端引导重新选择角色。
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
         * @description 仅返回本人未作废且未到绝对到期时间的登录会话，不含模拟会话。
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
         * @description 成功返回访问令牌。网页客户端的刷新令牌存入 HttpOnly Cookie，响应体中的 refresh_token 为 null；移动端通过响应体获取。账号不存在、密码错误或账号禁用时统一返回 401；登录尝试受限时返回 429。
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
         * @description 作废当前会话，成功后清除刷新令牌 Cookie。已退出的访问令牌不能再次调用此接口。
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
         * @description 网页客户端通过 HttpOnly Cookie 提交刷新令牌，须携带可信 Origin 和 X-Requested-With: kmic；移动端通过请求体中的 refreshToken 提交。刷新成功后更换刷新令牌；未过期的旧令牌在宽限期外重用时，作废会话并返回 401。
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
         * 结束当前用户的登录会话
         * @description 只能结束当前用户的登录会话，模拟会话除外。会话不存在、属于其他用户或为模拟会话时均返回 404。结束当前会话相当于退出登录。身份校验通过后，目标会话已撤销时仍返回 204，不重复记录撤销事件。
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
         * @description 切换到用户已获授且启用的角色，返回新访问令牌。令牌角色与会话当前角色不一致时，请求会被拒绝。当前角色被收回或停用后仍可重新选择；模拟会话不能切换角色。
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
    "/api/v1/identity/authorization/explain": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * 授权诊断
         * @description 说明用户以某个角色登录时有哪些权限点；传了范围对象时，再说明这个对象的数据范围等级来自角色默认还是单独设置、组织起点来自授予锚点还是所属组织、能看到多少组织单元。用户不能以这个角色登录时返回原因。用户或角色不存在返回 404，范围对象不存在返回 400。查询会记录读取审计。
         */
        get: {
            parameters: {
                query: {
                    /** @description 用户 ID；必填 */
                    UserId: string;
                    /** @description 角色 ID；必填，可以是用户没有被授予的角色 */
                    RoleId: string;
                    /** @description 范围对象编码，如 Sample.Specimen；不传时不返回数据范围 */
                    ScopeObject?: string;
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
                        "application/json": components["schemas"]["AuthorizationExplanation"];
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
         * 结束当前模拟会话
         * @description 须使用模拟令牌调用，非模拟会话返回 422 Identity.NotImpersonating。只读模拟会话也可调用，结束后前端须丢弃模拟令牌。
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
         * @description 创建独立的模拟会话，不影响管理员原会话。仅返回访问令牌，无刷新令牌，有效期为会话上限（默认 1 小时）。请求以目标用户身份执行，审计另记实际操作人。默认只读，具备“模拟身份时允许写入”权限时可修改数据。不能模拟自己、系统账号或受保护角色持有人，也不能在模拟会话中再次模拟。无需幂等键。
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
         * 设置用户所属组织
         * @description 未指定角色授予锚点时，“本组织”和“本组织及下级”以用户所属组织为起点。修改所属组织不会变更已有业务数据的归属组织。
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
         * @description 在已启用的组织单元下新增下级，排在同级末尾。响应中的 version 是新增后的组织树版本号。必须提供幂等键。
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
         * @description 存在启用的下级或启用的所属用户时，返回 409 Identity.OrgUnitInUse。根单元不能停用。
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
         * @description 上级组织单元必须已启用。
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
         * 查询完整组织树
         * @description 登录即可查询，返回包含停用单元的平铺列表，按上级分组、同级按顺序排列。前端通过 parentId 组装树；修改组织树时须提交返回的 version。
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
         * @description 变更上级后排在新同级末尾；上级未变时不调整顺序。不能移到自身或自身下级中，根单元不能移动。
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
         * 调整直属下级的顺序
         * @description 必须提交指定组织单元的全部直属下级，且不能重复。
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
         * @description 分页返回角色、状态、是否受保护及持有人数。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在名称和说明里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，多个值须重复参数名，如 status=Active&amp;status=Disabled */
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
         * @description 新角色不含权限点，默认数据范围按请求设置。必须提供幂等键。
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
         * @description 该角色从下一次请求起不可用，用户须重新选择角色。内置系统管理员角色不能停用。
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
         * @description 返回权限（含已退役标记）、数据范围配置、持有人数、版本号和当前用户的可用操作。
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
         * 修改角色名称、说明和顺序
         * @description 修改受保护角色时，操作者的当前角色也必须受保护。
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
         * @description 返回可单独配置数据范围等级的对象。
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
         * @description 返回各模块声明的权限，按模块和分组排列。
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
         * 替换角色数据范围
         * @description 包含默认范围等级及各对象单独设置的等级。所有数据范围对象都必须已在目录中声明。
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
         * 替换角色权限
         * @description 仅接受权限目录中已声明的编码。内置系统管理员角色拥有全部权限，不能手动修改其权限配置。
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
         * @description 分页查询登录失败、越权访问、授权变更和模拟身份等安全事件。时间范围必填，最长 31 天，默认按时间倒序。查询属于敏感操作，会记录读取审计。
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
                    /** @description 按事件关联的账号筛选 */
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
         * @description 分页返回账号、姓名、工号、手机号、状态、所属组织和角色，不包含系统账号。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 关键字，在账号、姓名、英文名、工号、手机号里模糊匹配 */
                    Q?: string;
                    /** @description 状态编码，多个值须重复参数名，如 status=Active&amp;status=Disabled */
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
         * @description 创建时不设置密码、所属组织或角色，需通过对应接口配置。账号、手机号或工号重复时返回对应的 409 错误。必须提供幂等键。
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
         * @description 禁用后，该用户的全部会话失效。不能禁用自己，也不能禁用最后一位可登录且持有已启用受保护角色的用户。
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
         * @description 结束目标用户的未作废会话，包括以其身份创建或由其发起的模拟会话；可指定一个会话或处理全部。不能对自己执行此操作，请通过“我的登录设备”管理自己的会话。目标用户持有受保护角色时，操作者的当前角色也必须受保护。
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
         * @description 返回用户资料、角色授予及组织锚点、登录限制截止时间、有效会话数、版本号和当前用户的可用操作。
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
         * @description 整体提交资料字段。修改账号或手机号时，若目标用户持有受保护角色，操作者的当前角色也必须受保护。
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
         * @description 角色及组织锚点必须已启用。不能修改自己的角色授予；授予受保护角色时，操作者的当前角色也必须受保护。必须提供幂等键。
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
         * 查询用户有效会话
         * @description 仅返回未作废且未到绝对到期时间的会话，包含以该用户身份创建或由其发起的模拟会话；返回设备、IP、登录及最近刷新时间。
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
         * @description 生成临时密码，仅在本次响应中返回。该用户的全部会话失效，下次登录必须修改密码。无需幂等键，重复提交会生成新密码，旧临时密码随即失效。目标用户持有受保护角色时，操作者的当前角色也必须受保护。
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
         * @description 该用户从下一次请求起不能再使用此角色，须重新选择角色。不能修改自己的角色授予；收回受保护角色时，操作者的当前角色也必须受保护。须保留至少一位可登录且持有已启用受保护角色的用户。
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
         * 查询用户选择列表
         * @description 登录即可查询，按姓名排序并分页。返回用户 ID、姓名、工号、所属组织和状态，不含手机号和邮箱。
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
         * 替换角色授予的组织锚点
         * @description 不能修改自己的角色授予。目标角色受保护时，操作者的当前角色也必须受保护。所有锚点都必须是已启用的组织单元。
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
         * @description 清除登录失败计数及登录限制，不影响已有会话，也不会启用已禁用账号。
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
         * 为示例添加附件
         * @description 先通过上传接口上传文件（上传配置 Sample.SpecimenAttachment），再使用返回的文件 ID 添加附件。只有负责人可以添加，示例关闭后不能添加。支持本人上传且可用的临时文件，以及已解除关联、尚未进入清理流程的文件。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 关闭示例
         * @description 只有负责人可以关闭已提交的示例。
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 查询示例列表
         * @description 分页返回当前用户有权查看的示例。页码超过末页返回空列表，每页条数须为 1 到 200，否则返回 400。
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
         * 创建示例
         * @description 创建草稿，负责人为当前用户。必须提供幂等键，超时重试时使用同一个键。成功返回 201，Location 指向新建示例的地址。
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
         * 查询示例详情
         * @description 返回示例信息、明细、附件、并发版本号和当前用户的可用操作。示例不存在或不在当前用户的数据范围内时返回 404。
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 删除示例
         * @description 仅支持软删除草稿。成功返回 204。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 下载示例附件
         * @description 只有能查看示例的用户才能下载其附件。示例不存在或不在当前用户的数据范围内时返回 404。默认下载文件；PDF 和图片可通过 inline=true 在浏览器中打开。支持 ETag；本地存储支持 Range，S3 存储返回完整文件。成功获取下载信息后记录审计日志。文件尚未就绪返回 409，已彻底删除返回 410。
         */
        get: {
            parameters: {
                query?: {
                    /** @description 在浏览器里直接打开；只有 PDF 和图片有效 */
                    inline?: boolean;
                };
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 移除示例附件
         * @description 只有负责人可以移除附件，示例关闭后不能移除。文件的全部引用解除后进入保留期，期间可以重新关联。成功返回 204。
         */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description 示例 ID */
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
         * 提交示例
         * @description 只有负责人可以提交草稿，且至少需要一项明细。成功后状态变为已提交。
         */
        post: {
            parameters: {
                query?: never;
                header: {
                    /** @description 幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求 */
                    "Idempotency-Key": string;
                };
                path: {
                    /** @description 示例 ID */
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
             * @description 字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 编码，字母、数字、点、短横线、下划线，1 到 50 位，创建后不能改 */
            code: string;
            /** @description 显示名，最多 200 个字符 */
            name: string;
            /** @description 父项编码；仅层级字典可指定 */
            parentCode: null | string;
            /**
             * Format: int32
             * @description 排序值；未指定时排在同级末尾
             */
            sortOrder: null | number;
        };
        /** @description 添加示例附件的请求 */
        AddSpecimenAttachmentRequest: {
            /**
             * Format: uuid
             * @description 上传得到的文件 ID
             */
            fileId: string;
            /**
             * Format: int64
             * @description 读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 设置用户所属组织单元的请求 */
        AssignUserOrgUnitRequest: {
            /**
             * Format: int64
             * @description 用户的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 组织单元，须启用；未指定时清空所属组织
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
             * @description 操作发生时间
             */
            occurredAt: string;
            /**
             * Format: date-time
             * @description 审计汇总接收时间
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
             * @description 操作人 ID
             */
            actorUserId: null | string;
            /** @description 操作人的名称；用户已经不存在时为空 */
            actorName: null | string;
            /**
             * Format: uuid
             * @description 模拟身份时的实际操作人 ID
             */
            actualUserId: null | string;
            /** @description 实际操作人的名称 */
            actualName: null | string;
            /**
             * Format: uuid
             * @description 操作时使用的角色 ID
             */
            roleId: null | string;
            /** @description 角色名称；角色已经不存在时为空 */
            roleName: null | string;
            /**
             * Format: uuid
             * @description 会话 ID
             */
            sessionId: null | string;
            /**
             * Format: uuid
             * @description 发起原始请求的用户
             */
            onBehalfOfUserId: null | string;
            /** @description 客户端 IP */
            clientIp: null | string;
            /** @description 链路追踪 ID，用于关联日志和调用链 */
            traceId: string;
            /** @description 操作对象 */
            targets: components["schemas"]["AuditTargetItem"][];
            /** @description 字段变更：实体、变更类型、各字段新旧值，敏感字段已脱敏 */
            changes: unknown;
            /** @description 补充说明，如导出的筛选条件 */
            details: unknown;
        };
        /** @description 审计记录列表项 */
        AuditListItem: {
            /**
             * Format: uuid
             * @description 审计记录 ID
             */
            id: string;
            /**
             * Format: date-time
             * @description 操作发生时间
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
             * @description 操作人 ID
             */
            actorUserId: null | string;
            /** @description 操作人的名称；用户已经不存在时为空 */
            actorName: null | string;
            /**
             * Format: uuid
             * @description 模拟身份时的实际操作人 ID
             */
            actualUserId: null | string;
            /** @description 实际操作人的名称 */
            actualName: null | string;
            /** @description 链路追踪 ID，用于关联日志和调用链 */
            traceId: string;
            /** @description 操作对象 */
            targets: components["schemas"]["AuditTargetItem"][];
        };
        /** @description 审计记录中的操作对象 */
        AuditTargetItem: {
            /** @description 资源类型，如 User */
            resourceType: string;
            /**
             * Format: uuid
             * @description 资源 ID
             */
            resourceId: string;
        };
        /** @description 授权诊断结果 */
        AuthorizationExplanation: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            userId: string;
            /** @description 用户姓名 */
            userName: string;
            /**
             * Format: uuid
             * @description 角色 ID
             */
            roleId: string;
            /** @description 角色名称 */
            roleName: string;
            /**
             * Format: int64
             * @description 角色版本号，改了角色的权限或数据范围就会变
             */
            roleVersion: number;
            /** @description 用户现在能否以这个角色登录 */
            usable: boolean;
            /** @description 不能登录的原因，能登录时为空 */
            unusableReason: null | string;
            /** @description 权限点编码，按编码排序；不能登录时为空集合 */
            permissions: string[];
            dataScope: null | components["schemas"]["DataScopeExplanation"];
        };
        /** @description 认证令牌响应 */
        AuthTokens: {
            /** @description 访问令牌 */
            access_token: string;
            /** @description 固定为 Bearer */
            token_type: string;
            /**
             * Format: int32
             * @description 访问令牌有效期（秒）
             */
            expires_in: number;
            /** @description 刷新令牌；没有新签发时为空 */
            refresh_token: null | string;
            /** @description 是否必须先修改密码 */
            passwordChangeRequired: boolean;
        };
        /** @description 后台作业信息，不含作业参数 */
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
            /** @description 作业显示名称；代码中已没有这种作业时为作业类型名 */
            jobName: string;
            /** @description 状态 */
            status: components["schemas"]["BackgroundJobStatus"];
            /**
             * Format: uuid
             * @description 提交人 ID，系统提交时为空
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
             * @description 首次执行时间
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
             * @description 总数量，未知时为空
             */
            progressTotal: null | number;
            /** @description 进度说明 */
            progressMessage: null | string;
            /**
             * Format: uuid
             * @description 结果文件 ID
             */
            resultFileId: null | string;
            /** @description 结果摘要（JSON） */
            resultSummary: unknown;
            /** @description 失败时的错误码 */
            errorCode: null | string;
            /** @description 失败时的错误说明 */
            errorMessage: null | string;
            /**
             * Format: int32
             * @description 执行尝试次数，每次领取时加一
             */
            attempts: number;
            /**
             * Format: date-time
             * @description 取消请求时间
             */
            cancelRequestedAt: string;
            /**
             * Format: uuid
             * @description 请求取消的用户 ID
             */
            cancelRequestedBy: null | string;
            /** @description 请求取消的用户名称 */
            cancelRequestedByName: null | string;
        };
        /**
         * @description 后台作业的状态
         * @enum {string}
         */
        BackgroundJobStatus: "Queued" | "Running" | "Succeeded" | "Failed" | "Cancelling" | "Cancelled";
        /** @description 被死信阻塞的顺序键 */
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
             * @description 等待最早死信处理完成的消息数
             */
            waiting: number;
            /**
             * Format: uuid
             * @description 最早死信的记录 ID，用于重试或放弃；处理后可推进该顺序键
             */
            firstDeadId: string;
            /** @description 最早死信的最近错误 */
            lastError: null | string;
        };
        /** @description 捕获内容详情 */
        CapturedDetailResponse: {
            /**
             * Format: uuid
             * @description 捕获记录 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 投递 ID
             */
            deliveryId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 收件地址（未脱敏） */
            address: string;
            /** @description 邮件收件方式：To、Cc 或 Bcc */
            role: components["schemas"]["DeliveryRole"];
            /** @description 主题 */
            subject: string;
            /** @description 正文 */
            body: string;
            /**
             * Format: date-time
             * @description 捕获时间
             */
            capturedAt: string;
        };
        /** @description 捕获内容列表项，不含正文 */
        CapturedListItem: {
            /**
             * Format: uuid
             * @description 捕获记录 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 投递 ID
             */
            deliveryId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 收件地址（未脱敏） */
            address: string;
            /** @description 邮件收件方式：To、Cc 或 Bcc */
            role: components["schemas"]["DeliveryRole"];
            /** @description 主题 */
            subject: string;
            /**
             * Format: date-time
             * @description 捕获时间
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
            /** @description 相关接口响应的追踪 ID，用于关联后端调用链，须为 32 位小写十六进制 */
            traceId: null | string;
            /**
             * Format: date-time
             * @description 浏览器中的错误发生时间
             */
            occurredAt: string;
        };
        /**
         * @description 客户端类型，决定刷新令牌怎样存放、滑动与绝对有效期多长
         * @enum {string}
         */
        ClientType: "Web" | "App";
        /** @description 关闭示例的请求 */
        CloseSpecimenRequest: {
            /**
             * Format: int64
             * @description 读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict
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
             * @description 组织树的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 上级组织单元，必须启用
             */
            parentId: string;
            /** @description 名称，最多 100 个字符；同级不得重名（含停用单元，不区分大小写） */
            name: string;
            /** @description 编码，最多 50 个字符，可以不填 */
            code: null | string;
        };
        /** @description 新建角色的请求 */
        CreateRoleRequest: {
            /** @description 名称，最多 100 个字符；不得重复（不区分大小写） */
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
        /** @description 创建示例时的一项明细 */
        CreateSpecimenItemRequest: {
            /** @description 名称，最多 50 个字符 */
            name: string;
            /**
             * Format: int32
             * @description 数量，1 到 9999
             */
            quantity: number;
        };
        /** @description 创建示例的请求 */
        CreateSpecimenRequest: {
            /** @description 标题，最多 100 个字符 */
            title: string;
            /** @description 明细，至少一项，最多 50 项 */
            items: components["schemas"]["CreateSpecimenItemRequest"][];
        };
        /** @description 新建用户的请求 */
        CreateUserRequest: {
            /** @description 登录名，最多 100 个字符；不得重复（不区分大小写） */
            account: string;
            /** @description 姓名，最多 100 个字符 */
            displayName: string;
            /** @description 英文名，最多 100 个字符 */
            englishName: null | string;
            /** @description 工号，最多 50 个字符，填写时唯一 */
            employeeNo: null | string;
            /** @description 手机号，填写时唯一；大陆号码可省略 +86 */
            mobile: null | string;
            /** @description 邮箱，最多 200 个字符 */
            email: null | string;
        };
        /** @description Cron 触发时间预览 */
        CronPreviewResponse: {
            /** @description 后续触发时间，最多 5 次，按时间升序 */
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
        /** @description 一个范围对象的数据范围及其来源 */
        DataScopeExplanation: {
            /** @description 范围对象编码 */
            scopeObject: string;
            /** @description 范围对象的显示名 */
            displayName: string;
            /** @description 等级 */
            level: components["schemas"]["RoleScopeLevel"];
            /** @description 等级是否为角色对这个范围对象单独设置的；为否时用角色的默认等级 */
            levelOverridden: boolean;
            /** @description 组织范围的起点；“本人”“全部”不按组织判断，为空集合 */
            anchors: components["schemas"]["ExplainedOrgUnit"][];
            /** @description 起点是否用了用户角色授予上设置的组织锚点；为否时用用户所属组织 */
            anchorsFromGrant: boolean;
            /**
             * Format: int32
             * @description 能看到的组织单元数，包括展开的下级；“本人”“全部”为空
             */
            orgUnitCount: null | number;
        };
        /** @description 数据范围对象目录项 */
        DataScopeObjectItem: {
            /** @description 编码 */
            code: string;
            /** @description 所属模块 */
            module: string;
            /** @description 显示名 */
            displayName: string;
        };
        /** @description 死信或已放弃消息的详情，包含概要、载荷和信封 */
        DeadMessageDetailItem: {
            /** @description 概要 */
            message: components["schemas"]["DeadMessageItem"];
            /** @description 状态：死信，或者已经被放弃 */
            state: components["schemas"]["MessageState"];
            /** @description 消息载荷；含密消息和目录里找不到的消息不返回，为空 */
            payload: unknown;
            /** @description 信封里的其余字段 */
            headers: unknown;
        };
        /** @description 死信列表项 */
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
             * @description 消息发生时间
             */
            occurredAt: string;
            /**
             * Format: int32
             * @description 处理尝试次数
             */
            attempts: number;
            /** @description 最近一次的错误 */
            lastError: null | string;
        };
        /** @description 删除示例的请求 */
        DeleteSpecimenRequest: {
            /**
             * Format: int64
             * @description 读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /**
         * @description 投递的渠道：站内消息不进投递队列，所以这里只有邮件和短信
         * @enum {string}
         */
        DeliveryChannel: "Email" | "Sms";
        /** @description 投递详情 */
        DeliveryDetailResponse: {
            /**
             * Format: uuid
             * @description 投递 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 产生它的发送请求
             */
            notificationId: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 邮件收件方式：To、Cc 或 Bcc */
            role: components["schemas"]["DeliveryRole"];
            /** @description 状态 */
            status: components["schemas"]["DeliveryStatus"];
            skipReason: null | components["schemas"]["DeliverySkipReason"];
            /**
             * Format: uuid
             * @description 系统内收件人 ID；外部收件人为空
             */
            recipientUserId: string;
            /** @description 脱敏后的收件地址 */
            addressMasked: string;
            /** @description 渲染后的主题 */
            subject: string;
            /** @description 渲染后的正文；含密投递发送成功、最终失败或跳过后为空 */
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
             * @description 领取次数，人工重试时清零
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
        /** @description 投递列表项，不含正文 */
        DeliveryListItem: {
            /**
             * Format: uuid
             * @description 投递 ID
             */
            id: string;
            /** @description 渠道 */
            channel: components["schemas"]["DeliveryChannel"];
            /** @description 状态 */
            status: components["schemas"]["DeliveryStatus"];
            skipReason: null | components["schemas"]["DeliverySkipReason"];
            /**
             * Format: uuid
             * @description 系统内收件人 ID；外部收件人为空
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
             * @description 领取次数，人工重试时清零
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
         * @description 邮件收件方式
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
        /** @description 批量字典查询结果 */
        DictionariesResponse: {
            /** @description 字典快照，按请求中的类型顺序排列 */
            dictionaries: components["schemas"]["DictionarySnapshot"][];
            /** @description 字典版本标识，用于条件请求；与 If-None-Match 匹配时返回 304 */
            eTag: string;
        };
        /** @description 字典项 */
        DictionaryEntry: {
            /** @description 字典项编码，供业务数据保存 */
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
            /** @description 是否启用；已停用项仍用于历史数据显示，不可用于新选择 */
            isActive: boolean;
        };
        /** @description 停用、启用字典项的请求 */
        DictionaryItemVersionRequest: {
            /**
             * Format: int64
             * @description 字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 字典类型快照，包含类型信息、版本号及全部字典项（含已停用项） */
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
            /** @description 全部字典项，按父项和排序值排列 */
            items: components["schemas"]["DictionaryEntry"][];
        };
        /** @description 字典类型详情 */
        DictionaryTypeDetail: {
            /** @description 类型编码 */
            code: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明字典类型的模块 */
            ownerModule: string;
            /** @description 是否层级字典 */
            isHierarchical: boolean;
            /** @description 状态：使用中、已退役 */
            status: components["schemas"]["DictionaryTypeStatus"];
            /**
             * Format: int64
             * @description 字典类型版本号，输出为 version，修改字典项时用于并发校验
             */
            version: number;
            /** @description 全部字典项，按父项和排序值排列 */
            items: components["schemas"]["DictionaryEntry"][];
        };
        /** @description 字典类型列表项 */
        DictionaryTypeListItem: {
            /** @description 类型编码 */
            code: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明字典类型的模块 */
            ownerModule: string;
            /** @description 是否层级字典 */
            isHierarchical: boolean;
            /** @description 状态：使用中、已退役 */
            status: components["schemas"]["DictionaryTypeStatus"];
            /**
             * Format: int32
             * @description 字典项总数，包含已停用项
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
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 原因，记入状态历史 */
            reason: null | string;
        };
        /** @description 禁用用户的请求 */
        DisableUserRequest: {
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 原因，记入状态历史 */
            reason: null | string;
        };
        /** @description 启用角色的请求 */
        EnableRoleRequest: {
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 启用用户的请求 */
        EnableUserRequest: {
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 组织单元 */
        ExplainedOrgUnit: {
            /**
             * Format: uuid
             * @description 组织单元 ID
             */
            id: string;
            /** @description 名称；组织单元已删除时为空 */
            name: null | string;
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
             * @description 模拟身份时的实际操作人
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
        /** @description 当前已开启的功能开关 */
        FeaturesResponse: {
            /** @description 开启的开关名，按名称排序；没有配置的开关按关闭处理，不在列表里 */
            enabled: string[];
        };
        /** @description 文件详情，不含内容或存储键 */
        FileDetail: {
            /**
             * Format: uuid
             * @description 文件 ID
             */
            id: string;
            /**
             * Format: int64
             * @description 并发版本号，修改时通过 expectedVersion 提交
             */
            version: number;
            /** @description 显示文件名 */
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
            /** @description 上传配置标识；生成文件为 Generated:来源 */
            profile: string;
            /**
             * Format: uuid
             * @description 上传人或生成请求人 ID，可为空
             */
            uploadedBy: string;
            /**
             * Format: date-time
             * @description 文件登记时间
             */
            uploadedAt: string;
            /** @description 状态 */
            status: components["schemas"]["StoredFileStatus"];
            /**
             * Format: date-time
             * @description 清理到期时间
             */
            expiresAt: string;
            /** @description 拒绝原因 */
            rejectReason: null | string;
            /**
             * Format: date-time
             * @description 彻底删除标记时间
             */
            purgedAt: string;
            /**
             * Format: uuid
             * @description 彻底删除操作人 ID
             */
            purgedBy: string;
            /** @description 彻底删除原因 */
            purgeReason: null | string;
            /**
             * Format: date-time
             * @description 对象删除登记时间
             */
            objectDeletedAt: string;
            /**
             * Format: date-time
             * @description 历史版本清除登记时间
             */
            historyPurgedAt: string;
            /** @description 历史版本清除记录 */
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
        /** @description 文件列表项，不含存储键 */
        FileListItem: {
            /**
             * Format: uuid
             * @description 文件 ID
             */
            id: string;
            /** @description 显示文件名 */
            name: string;
            /** @description 文件类型编码，未登记写入结果时为空 */
            fileType: null | string;
            /**
             * Format: int64
             * @description 文件大小（字节），未登记写入结果时为空
             */
            size: null | number;
            /** @description 上传配置标识；生成文件为 Generated:来源 */
            profile: string;
            /**
             * Format: uuid
             * @description 上传人或生成请求人 ID，可为空
             */
            uploadedBy: string;
            /**
             * Format: date-time
             * @description 文件登记时间
             */
            uploadedAt: string;
            /** @description 状态 */
            status: components["schemas"]["StoredFileStatus"];
            /**
             * Format: date-time
             * @description 清理到期时间
             */
            expiresAt: string;
            /**
             * Format: int64
             * @description 并发版本号
             */
            version: number;
        };
        /** @description 业务对象对文件的引用 */
        FileReferenceItem: {
            /** @description 引用文件的业务模块 */
            ownerModule: string;
            /** @description 业务对象类型 */
            ownerType: string;
            /**
             * Format: uuid
             * @description 业务对象的 ID
             */
            ownerId: string;
            /**
             * Format: date-time
             * @description 关联时间
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
             * @description 是否放弃未完成的消息，默认为 false；未选择放弃且有未完成消息时拒绝终结
             * @default false
             */
            abandonUnfinished: boolean;
        };
        /** @description 终结订阅的结果 */
        FinalizeRouteResponse: {
            /**
             * Format: int64
             * @description 终结时放弃的未完成消息数
             */
            abandoned: number;
        };
        /** @description 强制下线的请求 */
        ForceLogoutRequest: {
            /**
             * Format: uuid
             * @description 目标会话 ID；未指定时结束该用户的全部未作废会话，包括以其身份创建或由其发起的模拟会话
             */
            sessionId: string;
        };
        /** @description 授予角色的请求 */
        GrantRoleRequest: {
            /**
             * Format: int64
             * @description 用户的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 角色，必须启用
             */
            roleId: string;
            /** @description 组织锚点，须启用；未指定或为空列表时以用户所属组织为起点 */
            anchors: null | unknown[];
        };
        /** Format: binary */
        IFormFile: string;
        /** @description 运行实例及其订阅信息 */
        InstanceItem: {
            /**
             * Format: uuid
             * @description 实例 ID
             */
            id: string;
            /** @description 机器名 */
            host: string;
            /** @description 运行角色，如 Api、Worker */
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
            /** @description 心跳是否仍在有效期内 */
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
         * @description 定时任务触发方式
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
             * @description 本次标记为已读的消息数
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
             * @description 所属组织单元 ID
             */
            orgUnitId: null | string;
            /** @description 当前角色 */
            currentRole: components["schemas"]["MeRole"];
            /** @description 已获授且启用的角色 */
            roles: components["schemas"]["MeRole"][];
            /** @description 当前角色的权限点，按编码排序 */
            permissions: string[];
            /**
             * Format: int64
             * @description 授权版本号，变化时前端重新查询用户信息
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
             * @description 模拟身份时的实际操作人 ID
             */
            impersonatorUserId: null | string;
            /** @description 模拟会话是否允许修改数据 */
            impersonationAllowsWrite: boolean;
            /** @description 业务时区（IANA ID），前端统一按它显示时刻 */
            businessTimeZone: string;
        };
        /** @description 用户角色信息 */
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
         * @description 收发件箱类型
         * @enum {string}
         */
        MessageBoxKind: "Outbox" | "Inbox";
        /** @description 消息重试或放弃结果，各条结果独立，单条失败不影响其他消息 */
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
            /** @description 未成功原因，放弃检查拒绝时返回接收方提供的说明 */
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
         * @description 消息运维列表的状态
         * @enum {string}
         */
        MessageState: "Dead" | "Abandoned";
        /** @description 消息积压汇总 */
        MessageSummaryResponse: {
            /** @description 每个模块一项 */
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
             * @description 字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新父项编码；为 null 或空白时移至顶层，不能指定自身或后代 */
            newParentCode: null | string;
        };
        /** @description 移动组织单元的请求 */
        MoveOrgUnitRequest: {
            /**
             * Format: int64
             * @description 组织树的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /**
             * Format: uuid
             * @description 新的上级，必须启用，不能是自己或自己的下级
             */
            newParentId: string;
        };
        /** @description 本人的偏好项 */
        MyPreferenceItem: {
            /** @description 键 */
            key: string;
            /** @description 显示名称 */
            displayName: string;
            /** @description 是否偏好族键；后端仅校验 JSON 对象格式和大小，前端按版本化格式读取 */
            isFamily: boolean;
            /** @description 生效的值，JSON 对象 */
            value: unknown;
            /** @description 生效值的来源；所有允许层均无可用值时为默认 */
            source: components["schemas"]["PreferenceSource"];
            /** @description 当前用户是否可设置个人层的值 */
            canSet: boolean;
        };
        /** @description 本人的偏好 */
        MyPreferencesResponse: {
            /** @description 按键排序 */
            preferences: components["schemas"]["MyPreferenceItem"][];
        };
        /** @description 本人的登录会话及设备信息 */
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
             * @description 最近刷新时间
             */
            lastRefreshedAt: string;
            /** @description 是否当前会话 */
            isCurrent: boolean;
        };
        /** @description 代码中已移除设置的残留值 */
        ObsoleteSettingItem: {
            /** @description 设置键 */
            key: string;
            /** @description 已保存的值 */
            value: unknown;
            /**
             * Format: int64
             * @description 版本号，输出为 version，清除时须提交
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
            /** @description 全部组织单元的平铺列表，按上级和排序值排列，前端通过 parentId 构建树 */
            units: components["schemas"]["OrgUnitItem"][];
        };
        /** @description 组织单元列表项 */
        OrgUnitItem: {
            /**
             * Format: uuid
             * @description 组织单元 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 上级组织单元 ID，根单元为空
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
             * @description 组织树的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 发件箱的积压 */
        OutboxBacklogItem: {
            /**
             * Format: int64
             * @description 待投递和投递中的消息数，不含死信
             */
            pending: number;
            /**
             * Format: int64
             * @description 最早积压消息距发生时间的秒数，无积压时为 0
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
        /** @description 权限点目录项 */
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
        /** @description 偏好管理列表项 */
        PreferenceDefinitionItem: {
            /** @description 键 */
            key: string;
            /** @description 显示名称 */
            displayName: string;
            /** @description 所属模块；偏好族键的模块为 null */
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
            /** @description 普通偏好在前、偏好族在后，各组按模块和键排序 */
            preferences: components["schemas"]["PreferenceDefinitionItem"][];
        };
        /** @description 偏好在系统层或某个角色层上的值 */
        PreferenceLayerValue: {
            /** @description 值，JSON 对象 */
            value: unknown;
            /**
             * Format: int64
             * @description 版本号，修改或删除时须提交
             */
            version: number;
            /**
             * Format: uuid
             * @description 角色层所属的角色；系统层为空
             */
            roleId: null | string;
            /** @description 角色名称；系统层或角色已不存在时为空 */
            roleName: null | string;
        };
        /**
         * @description 偏好生效值的来源
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
            /** @description 渲染数据；未指定时使用模板示例数据，含密模板不得指定 */
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
             * @description 模板记录的预期并发版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion?: null | number;
        };
        /** @description 彻底删除文件的请求 */
        PurgeFileRequest: {
            /** @description 原因，必填 */
            reason: string;
            /**
             * Format: int64
             * @description 预期版本号，与当前版本不符时返回 409 Common.VersionConflict
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
        /** @description 读模型重建记录 */
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
             * @description 源端不存在的行数；等待确认时为当前已发现的数量
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
         * @description 读模型重建状态
         * @enum {string}
         */
        RebuildRunStatus: "Active" | "AwaitingConfirmation" | "Completed" | "Abandoned" | "Failed";
        /** @description 登记文件历史版本清除记录的请求 */
        RecordHistoryPurgeRequest: {
            /** @description 执行记录，如工单号，必填 */
            note: string;
            /**
             * Format: int64
             * @description 预期版本号，与当前版本不符时返回 409 Common.VersionConflict
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
             * @description 区间内符合补投条件的消息数（已投递）
             */
            candidates: number;
            /**
             * Format: int64
             * @description 实际写入收件箱的消息数，不含重复消息
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
        /** @description 刷新令牌请求；移动端提交请求体，网页客户端使用 Cookie */
        RefreshRequest: {
            /** @description 刷新令牌 */
            refreshToken: null | string;
        };
        /** @description 移除示例附件的请求 */
        RemoveSpecimenAttachmentRequest: {
            /**
             * Format: int64
             * @description 读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 修改字典项显示名的请求 */
        RenameDictionaryItemRequest: {
            /**
             * Format: int64
             * @description 字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的显示名，最多 200 个字符 */
            name: string;
        };
        /** @description 调整字典项次序的请求 */
        ReorderDictionaryItemsRequest: {
            /**
             * Format: int64
             * @description 字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 父项编码；未指定时调整顶层字典项 */
            parentCode: null | string;
            /** @description 按新顺序排列的全部同级字典项编码（含停用项），不得重复 */
            orderedCodes: string[];
        };
        /** @description 调整同级次序的请求 */
        ReorderOrgUnitsRequest: {
            /**
             * Format: int64
             * @description 组织树的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 按新顺序排列的全部直属下级 ID（含停用单元），不得重复 */
            orderedChildren: unknown[];
        };
        /** @description 前端批量上报错误的请求 */
        ReportClientErrorsRequest: {
            /** @description 本批错误，默认每次最多 20 条，实际以配置为准；超出时须分批上报。仅接受已声明字段，未知字段会导致整次请求被拒绝 */
            errors: components["schemas"]["ClientErrorReport"][];
        };
        /** @description 删除偏好在系统层或角色层的值 */
        ResetPreferenceLayerRequest: {
            /**
             * Format: int64
             * @description 预期版本号，记录已变更或删除时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 重置密码的结果 */
        ResetUserPasswordResponse: {
            /**
             * Format: uuid
             * @description 用户 ID
             */
            id: string;
            /** @description 临时密码，仅本次响应返回，不写入日志或审计 */
            temporaryPassword: string;
        };
        /** @description 标记存储问题已解决的请求 */
        ResolveStorageIssueRequest: {
            /** @description 处理说明，必填，如还原的工单号 */
            note: string;
            /**
             * Format: int64
             * @description 预期版本号，与当前版本不符时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 恢复定时任务默认值的请求 */
        RestoreJobDefaultRequest: {
            /**
             * Format: int64
             * @description 自定义设置的预期版本号（列表中的 overrideVersion），不匹配时返回 409 Common.VersionConflict
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
             * @description 模板记录的预期并发版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion?: null | number;
        };
        /** @description 收回角色的请求 */
        RevokeRoleRequest: {
            /**
             * Format: int64
             * @description 用户的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 角色对指定数据范围对象的等级配置 */
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
            /** @description 当前用户可执行的角色管理动作 */
            allowedActions: string[];
        };
        /** @description 角色列表项 */
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
        /** @description 角色拥有的权限点 */
        RolePermissionItem: {
            /** @description 权限点编码 */
            code: string;
            /** @description 显示名称，代码目录中已不存在的权限点为空 */
            displayName: null | string;
            /** @description 是否已从代码目录移除；替换权限点时不能提交此编码 */
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
        /** @description 声明订阅的构建信息 */
        RouteBuild: {
            /** @description 构建号 */
            build: string;
            /**
             * Format: int32
             * @description 声明该订阅的存活实例数
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
            /** @description 存活实例中声明该订阅的构建及实例数，供终结前检查 */
            declaredByBuilds: components["schemas"]["RouteBuild"][];
        };
        /** @description 定时任务及其默认计划、自定义设置、实际调度状态和最近执行记录 */
        ScheduledJobItem: {
            /** @description 稳定名称 */
            name: string;
            /** @description 所属模块；框架自带的任务是 Framework */
            module: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 任务是否仍在代码中声明；已移除任务保留自定义设置，便于回滚 */
            isDeclared: boolean;
            /** @description 代码默认 Cron 表达式；已移除声明的任务为空 */
            defaultCron: null | string;
            /** @description 自定义启用状态；为 null 时使用默认值（启用） */
            overrideEnabled: null | boolean;
            /** @description 自定义 Cron 表达式；为 null 时使用代码默认表达式 */
            overrideCron: null | string;
            /**
             * Format: int64
             * @description 自定义设置的版本号，修改时须提交；尚无自定义设置时为空
             */
            overrideVersion: null | number;
            /** @description 当前配置的启用状态 */
            enabled: boolean;
            /** @description 当前配置的 Cron 表达式 */
            cron: null | string;
            /** @description 调度器当前的 Cron 表达式，无触发器时为空 */
            activeCron: null | string;
            /**
             * Format: date-time
             * @description 下次计划触发时间；无后续触发或无触发器时为空
             */
            nextFireTime: string;
            /** @description 实际调度状态是否与当前配置一致 */
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
             * @description 计划触发时间
             */
            scheduledFireTime: string;
            /**
             * Format: date-time
             * @description 开始时间
             */
            startedAt: string;
            /**
             * Format: date-time
             * @description 结束时间；执行中为空
             */
            finishedAt: string;
            /**
             * Format: int64
             * @description 耗时，毫秒
             */
            durationMs: null | number;
            /** @description 触发方式：按计划、手动或补执行 */
            trigger: components["schemas"]["JobRunTrigger"];
            /**
             * Format: uuid
             * @description 手动触发用户 ID
             */
            triggeredBy: null | string;
            /** @description 手动触发用户名称，用户不存在时为空 */
            triggeredByName: null | string;
            /** @description 状态 */
            status: components["schemas"]["JobRunStatus"];
            /**
             * Format: int32
             * @description 已处理条数
             */
            itemsProcessed: null | number;
            /** @description 执行结果说明 */
            note: null | string;
            /** @description 失败时的错误码 */
            errorCode: null | string;
            /** @description 失败时的错误说明 */
            errorMessage: null | string;
            /** @description 执行任务的调度器实例 */
            instanceId: string;
            /** @description 链路追踪 ID */
            traceId: null | string;
        };
        /** @description 安全事件列表项 */
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
             * @description 用户 ID
             */
            userId: null | string;
            /**
             * Format: uuid
             * @description 实际操作人 ID
             */
            actualUserId: null | string;
            /**
             * Format: uuid
             * @description 操作时使用的角色 ID
             */
            roleId: null | string;
            /**
             * Format: uuid
             * @description 会话 ID
             */
            sessionId: null | string;
            /** @description 事件关联的账号 */
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
            /** @description 链路追踪 ID，用于关联请求日志和调用链 */
            traceId: string;
        };
        /**
         * @description 严重程度
         * @enum {unknown}
         */
        SecuritySeverity: "Info" | "Warning" | "High";
        /** @description 替换角色授予组织锚点的请求 */
        SetGrantAnchorsRequest: {
            /**
             * Format: int64
             * @description 用户的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的组织锚点，须启用；空列表表示清空锚点，改用用户所属组织 */
            anchors: unknown[];
        };
        /** @description 整体替换定时任务自定义设置，null 表示使用默认值 */
        SetJobOverrideRequest: {
            /** @description 是否启用，null 表示使用默认值（启用） */
            enabled: null | boolean;
            /** @description Cron 表达式，Quartz 写法（带秒），按业务时区解释；预览中的触发间隔至少 1 分钟，为 null 时使用代码默认表达式 */
            cron: null | string;
            /**
             * Format: int64
             * @description 自定义设置的预期版本号（列表中的 overrideVersion），尚无设置时不传；不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 保存本人偏好的请求 */
        SetMyPreferenceRequest: {
            /** @description 偏好值，须为 JSON 对象；已声明偏好按值类型解析并校验，偏好族仅保存 JSON 内容 */
            value: unknown;
        };
        /** @description 设置偏好在系统层或角色层的值 */
        SetPreferenceLayerRequest: {
            /** @description 偏好值，须为 JSON 对象；已声明偏好按值类型解析并校验，偏好族仅保存 JSON 内容 */
            value: unknown;
            /**
             * Format: int64
             * @description 预期版本号，当前层级尚无值时不传；不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 替换角色数据范围的请求 */
        SetRoleDataScopesRequest: {
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
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
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 新的权限点集合，只接受目录里存在的编码 */
            permissions: string[];
        };
        /** @description 修改系统设置的请求 */
        SetSettingRequest: {
            /** @description 新的设置值，须为 JSON 对象；字段及校验要求见设置定义 */
            value: unknown;
            /** @description 原因，必填，最多 500 个字符，记入变更历史 */
            reason: string;
            /**
             * Format: int64
             * @description 预期版本号，尚无保存值时不传；不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: null | number;
        };
        /** @description 设置变更记录 */
        SettingChangeItem: {
            /**
             * Format: uuid
             * @description 记录 ID
             */
            id: string;
            /** @description 设置键 */
            key: string;
            /** @description 修改前保存的值，原无保存记录时为空 */
            oldValue: unknown;
            /** @description 修改后保存的值；恢复默认或清除时为空 */
            newValue: unknown;
            /** @description 原因 */
            reason: string;
            /**
             * Format: uuid
             * @description 操作人 ID
             */
            changedBy: string;
            /** @description 操作人的姓名 */
            changedByName: null | string;
            /**
             * Format: date-time
             * @description 变更时间
             */
            changedAt: string;
            /** @description 链路追踪 ID */
            traceId: null | string;
        };
        /** @description 设置值里的一个成员：管理页据此生成编辑控件 */
        SettingFieldInfo: {
            /** @description 属性名转为 camelCase 后的名称 */
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
        /** @description 系统设置及其默认值、当前值和可用动作 */
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
            /** @description 管理页当前值，有保存值时返回保存内容，否则返回代码默认值 */
            currentValue: unknown;
            /** @description 是否已保存自定义值 */
            isModified: boolean;
            /**
             * Format: int64
             * @description 保存记录的版本号，输出为 version；未保存值时为空
             */
            version: null | number;
            /** @description 修改它需要的权限点 */
            editPermission: string;
            /** @description 当前用户可执行的动作 */
            allowedActions: string[];
        };
        /** @description 恢复默认、清除设置值的请求 */
        SettingReasonRequest: {
            /** @description 原因，必填，最多 500 个字符，记入变更历史 */
            reason: string;
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 全部系统设置 */
        SettingsResponse: {
            /** @description 代码里声明的设置，按模块、分组、键排好 */
            settings: components["schemas"]["SettingItem"][];
            /** @description 代码中已移除但仍有保存值的设置 */
            obsolete: components["schemas"]["ObsoleteSettingItem"][];
        };
        /** @description 站内消息列表项 */
        SiteMessageItem: {
            /**
             * Format: uuid
             * @description 消息 ID
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
             * @description 发送时间
             */
            sentAt: string;
            /** @description 是否已读 */
            isRead: boolean;
            /**
             * Format: date-time
             * @description 已读时间
             */
            readAt: string;
        };
        /** @description 示例附件信息，使用关联时保存的文件名、大小和类型 */
        SpecimenAttachmentResponse: {
            /**
             * Format: uuid
             * @description 附件 ID
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
        /** @description 示例的详情 */
        SpecimenDetailResponse: {
            /**
             * Format: uuid
             * @description 示例 ID
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
            /** @description 当前用户可执行的操作，如 Submit、Close */
            allowedActions: string[];
        };
        /** @description 示例明细 */
        SpecimenItemResponse: {
            /** @description 名称 */
            name: string;
            /**
             * Format: int32
             * @description 数量
             */
            quantity: number;
        };
        /** @description 示例列表项 */
        SpecimenListItem: {
            /**
             * Format: uuid
             * @description 示例 ID
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
         * @description 示例的状态
         * @enum {string}
         */
        SpecimenStatus: "Draft" | "Submitted" | "Closed";
        /** @description 开始模拟身份的请求 */
        StartImpersonationRequest: {
            /**
             * Format: uuid
             * @description 目标用户的角色 ID；未指定时优先使用上次角色，不可用时选择排序最前的启用角色
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
        /** @description 存储问题列表项 */
        StorageIssueItem: {
            /**
             * Format: uuid
             * @description 存储问题 ID
             */
            id: string;
            /**
             * Format: uuid
             * @description 文件 ID
             */
            fileId: string;
            /** @description 文件名；文件记录不存在时为空 */
            fileName: null | string;
            /** @description 问题类型 */
            kind: components["schemas"]["StorageIssueKind"];
            /** @description 发现时记录的对象版本，用于还原 */
            objectVersionId: null | string;
            /**
             * Format: date-time
             * @description 发现时间
             */
            foundAt: string;
            /** @description 状态 */
            status: components["schemas"]["StorageIssueStatus"];
            /**
             * Format: date-time
             * @description 标记已解决的时间
             */
            resolvedAt: string;
            /**
             * Format: uuid
             * @description 标记已解决的用户 ID
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
        /** @description 提交示例的请求 */
        SubmitSpecimenRequest: {
            /**
             * Format: int64
             * @description 读取时的版本号，与当前版本不一致时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
        };
        /** @description 一个订阅在收件箱里的积压 */
        SubscriptionBacklogItem: {
            /** @description 订阅名 */
            subscription: string;
            /**
             * Format: int64
             * @description 待处理和处理中的消息数，不含死信
             */
            pending: number;
            /**
             * Format: int64
             * @description 最早积压消息距接收时间的秒数，无积压时为 0
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
            /** @description 是否待终结；此类积压应单独告警 */
            isRetiring: boolean;
        };
        /** @description 切换角色的请求 */
        SwitchRoleRequest: {
            /**
             * Format: uuid
             * @description 目标角色 ID
             */
            roleId: string;
        };
        /**
         * @description 模板的渠道：模板可以发往哪些渠道，每个渠道的内容单独发布版本
         * @enum {string}
         */
        TemplateChannel: "Email" | "Sms" | "InApp";
        /** @description 模板渠道的内容 */
        TemplateChannelInfo: {
            /** @description 渠道编码：Email、Sms、InApp */
            channel: string;
            /**
             * Format: int32
             * @description 当前选用的发布版本号；未选用时为空
             */
            activeVersion: null | number;
            /** @description 当前的主题；站内消息里是标题 */
            subject: string;
            /** @description 当前的正文 */
            body: string;
            /** @description 代码默认主题；声明已移除时为空 */
            defaultSubject: null | string;
            /** @description 代码默认正文；声明已移除时为空 */
            defaultBody: null | string;
        };
        /** @description 通知模板详情 */
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
             * @description 并发版本号，以 version 输出；发布或恢复默认时用作预期版本号
             */
            version: number;
            /** @description 各渠道当前的内容 */
            channels: components["schemas"]["TemplateChannelInfo"][];
            /** @description 模板里能用的变量名 */
            variables: string[];
            /** @description 预览默认使用的示例数据；声明已移除时为空 */
            example: unknown;
            /** @description 已发布版本，按版本号倒序 */
            versions: components["schemas"]["TemplateVersionInfo"][];
        };
        /** @description 通知模板列表项 */
        TemplateListItem: {
            /** @description 模板键 */
            key: string;
            /** @description 显示名 */
            displayName: string;
            /** @description 声明它的模块 */
            module: string;
            /** @description 声明的渠道编码 */
            channels: string[];
            /** @description 内容含一次性短时凭据 */
            carriesSecret: boolean;
            /** @description 状态编码：Active、Retired */
            status: string;
            /** @description 各渠道选用的发布版本号；活动模板未选用版本时使用代码默认内容 */
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
        /** @description 模板预览结果 */
        TemplatePreviewResponse: {
            /** @description 渲染后的主题；渲染失败时为空 */
            subject: null | string;
            /** @description 渲染后的正文；渲染失败时为空 */
            body: null | string;
            /** @description 渲染失败原因；成功时为空 */
            error: null | string;
        };
        /** @description 模板发布版本 */
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
             * @description 组织树的预期版本号，不匹配时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
            /** @description 名称，最多 100 个字符；同级不得重名（含停用单元，不区分大小写） */
            name: string;
            /** @description 编码，最多 50 个字符；不填表示清空 */
            code: null | string;
        };
        /** @description 修改角色的请求 */
        UpdateRoleRequest: {
            /**
             * Format: int64
             * @description 预期版本号，不匹配时返回 409 Common.VersionConflict
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
             * @description 预期版本号，与当前版本不符时返回 409 Common.VersionConflict
             */
            expectedVersion: number;
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
        };
        /** @description 上传登记结果，包含文件 ID、存储键和清理后的文件名 */
        UploadStarted: {
            /**
             * Format: uuid
             * @description 文件 ID
             */
            fileId: string;
            /** @description 写入存储用的键 */
            storageKey: string;
            /** @description 清理后的文件名 */
            fileName: string;
        };
        /** @description 角色授予的组织锚点 */
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
             * @description 所属组织单元 ID
             */
            orgUnitId: string;
            /** @description 所属组织单元的名称 */
            orgUnitName: null | string;
            /** @description 是否已设置密码 */
            hasPassword: boolean;
            /** @description 下次登录是否必须先改密码 */
            passwordChangeRequired: boolean;
            /**
             * Format: date-time
             * @description 最近密码修改时间
             */
            passwordChangedAt: string;
            /**
             * Format: date-time
             * @description 登录等待或锁定的截止时间，无限制时为空；不影响已有会话
             */
            loginLockedUntil: string;
            /** @description 角色授予 */
            grants: components["schemas"]["UserGrantItem"][];
            /**
             * Format: int32
             * @description 未作废且未到绝对到期时间的会话数，含相关模拟会话
             */
            activeSessionCount: number;
            /** @description 当前用户可对该用户执行的管理动作 */
            allowedActions: string[];
        };
        /** @description 用户的角色授予信息 */
        UserGrantItem: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            roleId: string;
            /** @description 角色名称 */
            roleName: null | string;
            /** @description 角色是否启用，已停用角色不可用于登录或切换 */
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
             * @description 授予人 ID
             */
            grantedBy: string;
            /** @description 授予人的姓名 */
            grantedByName: null | string;
            /** @description 组织锚点；空列表时以用户所属组织为起点 */
            anchors: components["schemas"]["UserAnchorItem"][];
        };
        /** @description 用户列表项 */
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
             * @description 所属组织单元 ID
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
        /** @description 用户选择项 */
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
            /** @description 用户状态，供界面标记已禁用用户 */
            status: components["schemas"]["UserStatus"];
        };
        /** @description 用户列表中的角色摘要 */
        UserRoleBrief: {
            /**
             * Format: uuid
             * @description 角色 ID
             */
            id: string;
            /** @description 名称 */
            name: string;
            /** @description 角色是否启用，已停用角色不可用于登录或切换 */
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
             * @description 会话所属用户 ID，模拟会话为被模拟用户
             */
            userId: string;
            /**
             * Format: uuid
             * @description 模拟会话的实际操作人 ID，普通会话为空
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
             * @description 最近刷新时间
             */
            lastRefreshedAt: string;
            /**
             * Format: date-time
             * @description 刷新令牌到期时间；模拟会话为会话到期时间，不签发刷新令牌
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
