// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改

export type PermissionCode = "Configuration.Dictionary.Manage" | "Configuration.Preference.Manage" | "Configuration.Setting.Manage" | "Configuration.Setting.View" | "Files.File.Manage" | "Files.File.Purge" | "Files.File.View" | "Identity.Authorization.Explain" | "Identity.OrgUnit.Manage" | "Identity.Role.Manage" | "Identity.Role.View" | "Identity.SecurityEvent.View" | "Identity.User.ForceLogout" | "Identity.User.Grant" | "Identity.User.Impersonate" | "Identity.User.ImpersonateWrite" | "Identity.User.Manage" | "Identity.User.ResetPassword" | "Identity.User.Unlock" | "Identity.User.View" | "Notifications.Captured.View" | "Notifications.Delivery.Manage" | "Notifications.Delivery.View" | "Notifications.Template.Manage" | "Notifications.Template.View" | "Operations.Audit.Export" | "Operations.Audit.View" | "Operations.Jobs.Manage" | "Operations.Jobs.View" | "Operations.Messages.Manage" | "Operations.Messages.View" | "Operations.MessagingCapacity.Manage" | "Sample.Setting.Manage" | "Sample.Specimen.Close" | "Sample.Specimen.Create" | "Sample.Specimen.Delete" | "Sample.Specimen.Edit" | "Sample.Specimen.Submit" | "Sample.Specimen.View";

export const Permissions = {
  Configuration: {
    Dictionary: {
      Manage: "Configuration.Dictionary.Manage",
    },
    Preference: {
      Manage: "Configuration.Preference.Manage",
    },
    Setting: {
      Manage: "Configuration.Setting.Manage",
      View: "Configuration.Setting.View",
    },
  },
  Files: {
    File: {
      Manage: "Files.File.Manage",
      Purge: "Files.File.Purge",
      View: "Files.File.View",
    },
  },
  Identity: {
    Authorization: {
      Explain: "Identity.Authorization.Explain",
    },
    OrgUnit: {
      Manage: "Identity.OrgUnit.Manage",
    },
    Role: {
      Manage: "Identity.Role.Manage",
      View: "Identity.Role.View",
    },
    SecurityEvent: {
      View: "Identity.SecurityEvent.View",
    },
    User: {
      ForceLogout: "Identity.User.ForceLogout",
      Grant: "Identity.User.Grant",
      Impersonate: "Identity.User.Impersonate",
      ImpersonateWrite: "Identity.User.ImpersonateWrite",
      Manage: "Identity.User.Manage",
      ResetPassword: "Identity.User.ResetPassword",
      Unlock: "Identity.User.Unlock",
      View: "Identity.User.View",
    },
  },
  Notifications: {
    Captured: {
      View: "Notifications.Captured.View",
    },
    Delivery: {
      Manage: "Notifications.Delivery.Manage",
      View: "Notifications.Delivery.View",
    },
    Template: {
      Manage: "Notifications.Template.Manage",
      View: "Notifications.Template.View",
    },
  },
  Operations: {
    Audit: {
      Export: "Operations.Audit.Export",
      View: "Operations.Audit.View",
    },
    Jobs: {
      Manage: "Operations.Jobs.Manage",
      View: "Operations.Jobs.View",
    },
    Messages: {
      Manage: "Operations.Messages.Manage",
      View: "Operations.Messages.View",
    },
    MessagingCapacity: {
      Manage: "Operations.MessagingCapacity.Manage",
    },
  },
  Sample: {
    Setting: {
      Manage: "Sample.Setting.Manage",
    },
    Specimen: {
      Close: "Sample.Specimen.Close",
      Create: "Sample.Specimen.Create",
      Delete: "Sample.Specimen.Delete",
      Edit: "Sample.Specimen.Edit",
      Submit: "Sample.Specimen.Submit",
      View: "Sample.Specimen.View",
    },
  },
} as const;
