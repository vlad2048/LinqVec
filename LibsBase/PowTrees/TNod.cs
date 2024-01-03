using System.Collections;

// ReSharper disable once CheckNamespace
public static class Nod
{
	public static TNod<T> Make<T>(T v, IEnumerable<TNod<T>>? children = null) => new(v, children);
	public static TNod<T> Make<T>(T v, params TNod<T>[] children) => new(v, children);
}


public sealed class TNod<T> : IEnumerable<TNod<T>>
{
	private readonly List<TNod<T>> children = new();

	public T V { get; private set; }
	public TNod<T>? Parent { get; private set; }
	public IReadOnlyList<TNod<T>> Children => children;

	internal TNod(T v, IEnumerable<TNod<T>>? children)
	{
		V = v;
		if (children != null)
			foreach (var child in children)
			{
				this.children.Add(child);
				child.Parent = this;
			}
	}

	public override string ToString()
	{
		try
		{
			var str = $"{V}";
			return str;
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public void ChangeContent(T v) => V = v;

	public void AddChild(TNod<T> child)
	{
		children.Add(child);
		child.Parent = this;
	}

	public void InsertChild(TNod<T> child, int index)
	{
		children.Insert(index, child);
		child.Parent = this;
	}
	
	public void RemoveChild(TNod<T> child)
	{
		child.Parent = null;
		children.Remove(child);
	}

	public void ReplaceChild(TNod<T> childPrev, TNod<T> childNext)
	{
		var index = children.IndexOf(childPrev);
		if (index == -1) throw new ArgumentException();
		childPrev.Parent = null;
		children[index] = childNext;
		childNext.Parent = this;
	}

	public void ClearChildren()
	{
		foreach (var child in Children)
			child.Parent = null;
		children.Clear();
	}

	public void AddChildren(IEnumerable<TNod<T>> kids)
	{
		foreach (var kid in kids)
			AddChild(kid);
	}

	public IEnumerator<TNod<T>> GetEnumerator() => Enumerate();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private IEnumerator<TNod<T>> Enumerate()
	{
		IEnumerable<TNod<T>> Recurse(TNod<T> node)
		{
			yield return node;
			foreach (var child in node.Children)
			foreach (var childRes in Recurse(child))
				yield return childRes;
		}
		foreach (var res in Recurse(this))
			yield return res;
	}
}