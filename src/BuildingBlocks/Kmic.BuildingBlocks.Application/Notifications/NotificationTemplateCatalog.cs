using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.BuildingBlocks.Application.Notifications;

/// <summary>
/// 代码里声明的全部通知模板
/// </summary>
/// <remarks>
/// 在启动时从各模块的 Application 程序集里读取并核对：模板键以声明它的模块名开头且全系统唯一，至少有一个渠道，
/// 数据模型实现 INotificationModel，发短信的模型实现 ISmsTemplateModel，含密模板不发邮件。声明有误时宿主启动失败
/// </remarks>
public sealed class NotificationTemplateCatalog
{
    private static readonly MethodInfo _read = typeof(NotificationTemplateCatalog).GetMethod(nameof(ReadModel), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("找不到读取模板模型的方法。");

    private readonly FrozenDictionary<string, NotificationTemplateDefinition> _byKey;

    private NotificationTemplateCatalog(IEnumerable<NotificationTemplateDefinition> templates) =>
        _byKey = templates.ToFrozenDictionary(t => t.Key, StringComparer.Ordinal);

    /// <summary>
    /// 全部模板
    /// </summary>
    public IReadOnlyCollection<NotificationTemplateDefinition> All => _byKey.Values;

    /// <summary>
    /// 按键取模板，不存在返回空
    /// </summary>
    /// <param name="key">模板键</param>
    public NotificationTemplateDefinition? Find(string key) => _byKey.GetValueOrDefault(key);

    /// <summary>
    /// 从程序集里读取
    /// </summary>
    /// <param name="assemblies">各模块的 Application 程序集</param>
    /// <exception cref="DeclarationException">声明有误</exception>
    public static NotificationTemplateCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(NotificationTemplateAttribute), inherit: false)));
    }

    /// <summary>
    /// 从声明了模板的模型类型里读取
    /// </summary>
    /// <param name="modelTypes">标了 NotificationTemplate 的模型类型</param>
    /// <exception cref="DeclarationException">声明有误</exception>
    public static NotificationTemplateCatalog Build(IEnumerable<Type> modelTypes)
    {
        ArgumentNullException.ThrowIfNull(modelTypes);

        var problems = new List<string>();
        var templates = new List<NotificationTemplateDefinition>();
        foreach (var type in modelTypes.Distinct())
        {
            var attribute = type.GetCustomAttribute<NotificationTemplateAttribute>()
                ?? throw new ArgumentException($"{type.Name} 没有标 [NotificationTemplate]。", nameof(modelTypes));
            if (Read(type, attribute, problems) is { } definition)
            {
                templates.Add(definition);
            }
        }

        foreach (var duplicate in templates.GroupBy(t => t.Key, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"模板键 {duplicate.Key} 被 {string.Join("、", duplicate.Select(t => t.ModelType.Name))} 重复声明。");
        }

        return problems.Count > 0 ? throw new DeclarationException("通知模板声明", problems) : new NotificationTemplateCatalog(templates);
    }

    private static NotificationTemplateDefinition? Read(Type type, NotificationTemplateAttribute attribute, List<string> problems)
    {
        var name = type.Name;
        var module = ModuleNames.Of(type);
        var found = true;
        if (!attribute.Key.StartsWith(module + ".", StringComparison.Ordinal) || attribute.Key.Length <= module.Length + 1)
        {
            problems.Add($"{name}：模板键 {attribute.Key} 要以声明它的模块名“{module}.”开头。");
            found = false;
        }

        if (attribute.Channels == NotificationChannels.None)
        {
            problems.Add($"{name}：模板 {attribute.Key} 至少要有一个渠道。");
            found = false;
        }

        if (attribute.CarriesSecret && attribute.Channels.HasFlag(NotificationChannels.Email))
        {
            problems.Add($"{name}：含密模板 {attribute.Key} 不能发邮件，验证码这类凭据只走短信或站内消息。");
            found = false;
        }

        if (attribute.Channels.HasFlag(NotificationChannels.Sms) && !typeof(ISmsTemplateModel).IsAssignableFrom(type))
        {
            problems.Add($"{name}：模板 {attribute.Key} 发短信，数据模型要实现 ISmsTemplateModel 说明怎样变成短信模板的参数。");
            found = false;
        }

        var implements = type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationModel<>) && i.GetGenericArguments()[0] == type);
        if (!implements)
        {
            problems.Add($"{name}：数据模型要实现 INotificationModel<{name}>。");
            return null;
        }

        if (!found)
        {
            return null;
        }

        var (subject, body, text, example) = ((string, string, string, object))(_read.MakeGenericMethod(type).Invoke(null, null)
            ?? throw new InvalidOperationException($"读取 {name} 的模板内容失败。"));
        var variables = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).Order(StringComparer.Ordinal).ToList();
        return new NotificationTemplateDefinition(
            attribute.Key, attribute.DisplayName, module, attribute.Channels, attribute.SenderProfile, attribute.CarriesSecret, type, subject, body, text, example, variables);
    }

    // 静态抽象成员只能通过泛型读取，反射调用时给它一个具体的类型参数
    private static (string Subject, string Body, string Text, object Example) ReadModel<TModel>()
        where TModel : INotificationModel<TModel> =>
        (TModel.DefaultSubject, TModel.DefaultBody, TModel.DefaultTextBody, TModel.Example);
}
