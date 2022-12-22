namespace Porbeagle.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ActionRowAttribute : Attribute
{
    public ActionRowAttribute(int index) 
        => Index = index;

    public int Index { get; }
}