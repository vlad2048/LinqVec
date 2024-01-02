using System.Text.Json.Serialization;
using LogLib.Interfaces;
using LogLib.Writers;

namespace LogLib.Structs;

public class Txt
{
    public TxtSegment[][] Segments { get; }

    [JsonIgnore]
	public string Text
    {
        get
        {
	        var w = StringTxtWriter.Instance;
	        Segments.Run(w);
            return w.Text;
        }
    }

    internal Txt(TxtSegment[][] segments) => Segments = segments;

    public static Txt Make(Action<ITxtWriter> fun)
    {
        var writer = new MemoryTxtWriter();
        fun(writer);
        return writer.Txt;
    }

    public Txt Append(Txt other) => new([..Segments.Concat(other.Segments)]);
}
