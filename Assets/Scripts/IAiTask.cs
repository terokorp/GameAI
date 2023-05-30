using System.Collections;

public interface IAiTask
{
    internal IEnumerator DoTask(Character _character);
    internal int Priority { get; set; }
}