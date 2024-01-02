using System.Text;
using LogLib.Interfaces;
using LogLib.Structs;

namespace LogLib.Writers;

sealed class StringTxtWriter : ITxtWriter
{
	public static readonly StringTxtWriter Instance = new();
	private readonly StringBuilder sb = new();
	private StringTxtWriter() { }
	public int LastSegLength { get; private set; }
	public string Text => sb.ToString();
	public ITxtWriter Write(TxtSegment seg)
	{
		LastSegLength = seg.Text.Length;
		sb.Append(seg.Text);
		return this;
	}
	public ITxtWriter WriteLine()
	{
		LastSegLength = 0;
		sb.AppendLine();
		return this;
	}
}
