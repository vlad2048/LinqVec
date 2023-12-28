global using TDoc = VectorEditor.Model.Doc;
using LinqVec;
using VectorEditor;
using VectorEditor.Model;

namespace LinqVecDemo;

static class LogicSelector
{
	public static readonly EditorLogic<TDoc> Instance = new VectorEditorLogic();
}