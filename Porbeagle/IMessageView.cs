using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Porbeagle;

public interface IMessageView<out TSelf, in TViewModel>
{
    Optional<string> Text { get; init; }
    
    // These get generated from the view class' layout by a source generator; 
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }
    public Optional<IReadOnlyList<IMessageComponent>> Components { get; }
    public Optional<IReadOnlyList<Snowflake>> Stickers { get; }

    static abstract TSelf Create(TViewModel vm);
}