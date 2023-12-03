using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools.Curve_.Mods;

interface ICurveMod;
sealed record NoneCurveMod : ICurveMod;
sealed record AddPointCurveMod(Pt? StartPos) : ICurveMod;
sealed record MovePointCurveMod(PointId Id) : ICurveMod;
sealed record RemovePointCurveMod(int Idx) : ICurveMod;