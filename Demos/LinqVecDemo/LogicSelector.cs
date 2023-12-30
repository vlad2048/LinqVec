global using TDoc = VectorEditor._Model.Doc;
using LinqVec;
using VectorEditor;

namespace LinqVecDemo;

static class LogicSelector
{
	public static readonly EditorLogic<TDoc> Instance = new VectorEditorLogic();
}