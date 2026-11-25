// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改

export const enumLabels = {
  BackgroundJobStatus: [
    { code: "Queued", name: "排队中" },
    { code: "Running", name: "运行中" },
    { code: "Succeeded", name: "成功" },
    { code: "Failed", name: "失败" },
    { code: "Cancelling", name: "取消中" },
    { code: "Cancelled", name: "已取消" },
  ],
  ClientType: [
    { code: "Web", name: "网页" },
    { code: "App", name: "移动端" },
  ],
  DeliveryChannel: [
    { code: "Email", name: "邮件" },
    { code: "Sms", name: "短信" },
  ],
  DeliveryPriority: [
    { code: "Normal", name: "普通" },
    { code: "High", name: "高" },
  ],
  DeliveryRole: [
    { code: "To", name: "收件人" },
    { code: "Cc", name: "抄送" },
    { code: "Bcc", name: "密送" },
  ],
  DeliverySkipReason: [
    { code: "NoAddress", name: "没有地址" },
    { code: "UserDisabled", name: "用户已禁用" },
    { code: "UnknownUser", name: "用户不存在" },
  ],
  DeliveryStatus: [
    { code: "Pending", name: "待发送" },
    { code: "Sending", name: "发送中" },
    { code: "Sent", name: "已发送" },
    { code: "Failed", name: "失败" },
    { code: "Skipped", name: "已跳过" },
    { code: "Captured", name: "已捕获" },
  ],
  DictionaryTypeStatus: [
    { code: "Active", name: "使用中" },
    { code: "Retired", name: "已退役" },
  ],
  JobRunStatus: [
    { code: "Running", name: "执行中" },
    { code: "Succeeded", name: "成功" },
    { code: "Failed", name: "失败" },
    { code: "Unknown", name: "结果未知" },
    { code: "Abandoned", name: "中断" },
  ],
  JobRunTrigger: [
    { code: "Schedule", name: "按计划" },
    { code: "Manual", name: "手动执行" },
    { code: "Misfire", name: "补执行" },
  ],
  MessageBoxKind: [
    { code: "Outbox", name: "发件箱" },
    { code: "Inbox", name: "收件箱" },
  ],
  MessageReceiptStatus: [
    { code: "Done", name: "已处理" },
    { code: "NotFound", name: "不存在" },
    { code: "WrongState", name: "状态不对" },
    { code: "Refused", name: "被拒绝" },
  ],
  MessageRouteStatus: [
    { code: "Active", name: "生效" },
    { code: "Retiring", name: "待终结" },
    { code: "Finalized", name: "已终结" },
  ],
  MessageState: [
    { code: "Dead", name: "死信" },
    { code: "Abandoned", name: "已放弃" },
  ],
  OrgUnitStatus: [
    { code: "Active", name: "启用" },
    { code: "Disabled", name: "已停用" },
  ],
  PreferenceSource: [
    { code: "Default", name: "默认" },
    { code: "System", name: "系统" },
    { code: "Role", name: "角色" },
    { code: "UserRole", name: "本人" },
  ],
  RebuildRunStatus: [
    { code: "Active", name: "进行中" },
    { code: "AwaitingConfirmation", name: "等待确认" },
    { code: "Completed", name: "已完成" },
    { code: "Abandoned", name: "已放弃" },
    { code: "Failed", name: "已失败" },
  ],
  RoleScopeLevel: [
    { code: "Self", name: "仅本人" },
    { code: "OrgUnit", name: "本组织" },
    { code: "OrgUnitAndBelow", name: "本组织及下级" },
    { code: "All", name: "全部" },
  ],
  RoleStatus: [
    { code: "Active", name: "启用" },
    { code: "Disabled", name: "已停用" },
  ],
  SpecimenStatus: [
    { code: "Draft", name: "草稿" },
    { code: "Submitted", name: "已提交" },
    { code: "Closed", name: "已关闭" },
  ],
  StorageIssueKind: [
    { code: "RestorableMissing", name: "可以还原" },
    { code: "LostMissing", name: "已经丢失" },
  ],
  StorageIssueStatus: [
    { code: "Open", name: "待处理" },
    { code: "Resolved", name: "已解决" },
  ],
  StoredFileStatus: [
    { code: "Uploading", name: "上传中" },
    { code: "Scanning", name: "扫描中" },
    { code: "Available", name: "临时" },
    { code: "Attached", name: "已关联" },
    { code: "Detached", name: "已解除关联" },
    { code: "Rejected", name: "被拒绝" },
    { code: "Deleting", name: "删除中" },
    { code: "Purged", name: "已彻底删除" },
  ],
  TemplateChannel: [
    { code: "Email", name: "邮件" },
    { code: "Sms", name: "短信" },
    { code: "InApp", name: "站内消息" },
  ],
  UserStatus: [
    { code: "Active", name: "启用" },
    { code: "Disabled", name: "已禁用" },
  ],
} as const;

export type EnumName = keyof typeof enumLabels;
