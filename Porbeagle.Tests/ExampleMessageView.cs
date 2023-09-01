using Porbeagle.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Porbeagle.Tests;

public sealed record ExampleViewModel(string Description);

[DiscordView]
public partial record ExampleMessageView : IMessageView<ExampleMessageView, ExampleViewModel>
{
    public Optional<string> Text { get; init; } = "Some text";

    [Sticker]
    public Snowflake SomeSticker { get; } = new(5456508906066544);

    public Embed SomeEmbed1 { get; } = new();
    public Embed SomeEmbed2 { get; }

    [ActionRow(1)]
    public ButtonComponent ButtonComponent { get; } = new(ButtonComponentStyle.Success);
    
    [ActionRow(0)]
    public ButtonComponent ButtonComponent2 { get; } = new(ButtonComponentStyle.Success);
    
    [ActionRow(0)]
    public ButtonComponent ButtonComponent3 { get; } = new(ButtonComponentStyle.Success);

    private ExampleMessageView(ExampleViewModel vm) 
        => SomeEmbed2 = new(vm.Description);
}
