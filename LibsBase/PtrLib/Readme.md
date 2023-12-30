# PtrLib



## Synopsis
Library to update a model.
It was designed while writing a vector editing application.



## Usage


### 1) Support undo/redos.


### 2) ```V``` vs ```VModded```
Differentiates between:
- The official model: ```V```

  -> Used for the undo system.

  -> Used when saving/loading.
- A temp model including pending changes: ```VModded```

  -> Used for drawing.


### 3) Reactive changes
Add a reactive change to the model that returns an IDisposable.
When you create the reactive change it will be visible in ```VModded``` but not in ```V```.
The change has an ```Apply``` flag that determines what happens when you dispose the returned IDisposable:
  - ```Apply: false``` -> nothing
  - ```Apply: true``` -> commit the last change to ```V```

**Typical use case**
The user drags an element of the drawing to change its position:
  - We want to render the model while the drag is in progress: ```VModded```.
  - We only want to record the undo/redo operation when the drag is completed: ```V```.

&nbsp;

From now on, let's assume the model is an instance of the ```Dad``` class:
```c#
record Dad(Kid[] Kids);
record Kid(Guid Id, string Name);

// And we create a DadPtr holding it
var ptr = new DadPtr<Dad>(new Dad([]), d);
```

&nbsp;

### 4) Create new sub objects
We can create a new kid with a view to adding it to the model later:

**Signature**
```c#
class PtrDad {
	public PtrKidCreate<Dad, Kid> Create<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun
	);
}
```
**Parameters**
- ```init``` initial value of the Kid
- ```setFun``` how to we update the Dad to either
  - Add the Kid to the Dad.
  - Update the Kid's value in the Dad.
- ```validFun``` what states of the Kid are considered valid and can be added to the official model when committing the creation (see further).

**Important**
```setFun``` needs to be idempotent.

**Usage**
```c#
// Remember, the SetFun() function needs to be idempotent.
// One possible implementation would be:
// - If the kid is not in the dad (based on kid.Id) -> add the kid
// - If the kid is already in the dad (based on kid.Id) -> update the kid
Dad SetFun(Dad dad, Kid kid) {}

// One possible implementation of ValidFun would be to consider to kid valid if its name is not empty:
bool ValidFun(Kid kid) => kid.Name != "";

// Create a CreateKid
var kid = ptr.Create(new Kid(Guid.NewGuid(), ""), SetFun, kid => kid.Name != "");

// -> Right now neither dad.V nor .VModded will reflect the new kid

kid.V = kid.V with { Name = "Joh" }; // update the name of the kid
kid.V = kid.V with { Name = "John" }; // again

// Now you can either:

// 1) Decide you do not want to add the kid anymore
kid.Dispose();
//  -> The kid will never have been visible in the Dad nor its undo history

// 2) Decide you're happy with the kid and want to add it to the Dad
kid.Commit();
//  -> The kid is now be added to the Dad (both V & VModded)
//  -> The kid's undo history is now appended to the dad's undo history
//     (except for the changes where the kid is in an invalid state according to ValidFun)
//  -> The kid is now Disposed and you cannot use it anymore
```


### 5) Edit existing sub objects

We can edit an existing kid all the while hiding it from dad.VModded to provide specialized rendering while editing:

**Signature**
```c#
class PtrDad {
	public PtrKidEdit<Dad, Kid> Edit<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun
	);
}
```
**Parameters**
- ```init``` initial value of the Kid (needs to be the same as in the dad)
- ```setFun``` how to we update the Kid's value in the Dad
- ```removeFun``` how to remove the Kid from the Dad (only to hide it in dad.VModded)

**Usage**
```c#
var kidV = dad.V.Kids[2];

// Create an EditKid
var kid = dad.Edit(kidV, SetFun, RemoveFun);
// -> - The kid disappears from dad.VModded
//    - Any changes to the kid will be reflected in dad.V

// And finally when you're done with the edits, dispose the kid
kid.Dispose();
// -> The kid is now visible again in dad.VModded
```