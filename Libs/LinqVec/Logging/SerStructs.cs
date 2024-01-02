using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using LogLib;
using System.Text.Json.Serialization;
using LogLib.Interfaces;

namespace LinqVec.Logging;

// IEvt
[JsonDerivedType(typeof(MouseMoveEvt), typeDiscriminator: "MouseMoveEvt")]
[JsonDerivedType(typeof(MouseEnterEvt), typeDiscriminator: "MouseEnterEvt")]
[JsonDerivedType(typeof(MouseLeaveEvt), typeDiscriminator: "MouseLeaveEvt")]
[JsonDerivedType(typeof(MouseBtnEvt), typeDiscriminator: "MouseBtnEvt")]
[JsonDerivedType(typeof(MouseClickEvt), typeDiscriminator: "MouseClickEvt")]
[JsonDerivedType(typeof(MouseWheelEvt), typeDiscriminator: "MouseWheelEvt")]
[JsonDerivedType(typeof(KeyEvt), typeDiscriminator: "KeyEvt")]

// IUsr
[JsonDerivedType(typeof(MoveUsr), typeDiscriminator: "MoveUsr")]
[JsonDerivedType(typeof(LDownUsr), typeDiscriminator: "LDownUsr")]
[JsonDerivedType(typeof(LUpUsr), typeDiscriminator: "LUpUsr")]
[JsonDerivedType(typeof(RDownUsr), typeDiscriminator: "RDownUsr")]
[JsonDerivedType(typeof(RUpUsr), typeDiscriminator: "RUpUsr")]
[JsonDerivedType(typeof(KeyDownUsr), typeDiscriminator: "KeyDownUsr")]

// ICmdEvt
[JsonDerivedType(typeof(DragStartCmdEvt), typeDiscriminator: "DragStartCmdEvt")]
[JsonDerivedType(typeof(DragFinishCmdEvt), typeDiscriminator: "DragFinishCmdEvt")]
[JsonDerivedType(typeof(ConfirmCmdEvt), typeDiscriminator: "ConfirmCmdEvt")]
[JsonDerivedType(typeof(ShortcutCmdEvt), typeDiscriminator: "ShortcutCmdEvt")]
[JsonDerivedType(typeof(CancelCmdEvt), typeDiscriminator: "CancelCmdEvt")]

// Others
[JsonDerivedType(typeof(TimestampCon), typeDiscriminator: "TimestampCon")]
[JsonDerivedType(typeof(IsHotspotFrozenCon), typeDiscriminator: "IsHotspotFrozenCon")]
[JsonDerivedType(typeof(HotspotNameCon), typeDiscriminator: "HotspotNameCon")]

public interface IWriteSer : IWrite;

sealed record TimestampCon(DateTimeOffset Time) : IWriteSer
{
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

sealed record IsHotspotFrozenCon(bool Flag) : IWriteSer
{
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

sealed record HotspotNameCon(string Name) : IWriteSer
{
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}