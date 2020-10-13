# Ariadne
Ariadne is a simple .NET library which allows you to bind properties between each other in various ways. The simpliest way to establish a connection is using Binder class. Just like that:
```c#
delegate void PropertyChanged();

class WithA
{
	event PropertyChanged PropertyAChanged;

	float _a = 0;
	public float PropertyA
	{
		get => _a;
		set
		{
			_a = value;
			PropertyAChanged?.Invoke();
		}
	}
}

class WithB
{
	event PropertyChanged PropertyBChanged;

	float _b = 4.815162342f;
	public float PropertyB
	{
		get => _b;
		set
		{
			_b = value;
			PropertyBChanged?.Invoke();
		}
	}
}

IBinding binding;

void Bind()
{
	WithA a = new WithA();
	WithB b = new WithB();

	binding = Binder.Side(() => a.PropertyA).To(() => b.PropertyB).Using(BindingFlow.TwoWay);

	// Now we may call IBinding.Invalidate() method to assign a value of one property to another...
	binding.Invalidate(BindingSide.B); // PropertyA in a now equals 4.815162342f

	// ...And finally establish the connection...
	binding.Subscribe();

	// ...So now we simply can write
	a.PropertyA = 1.408f; // PropertyB in b now equals 4.815162342f	
}

void Dispose()
{
	// Don't forget to unsubscribe when you finished with your binding deals
	binding?.Unsubscribe();
}
```
We got 4 binding ways here:
- TwoWay – changing A changes B, changing B changes A;
- OneWay – all changes flow only from B to A;
- Reverse – changing A changes B, but if B has any logic and can alter the value, it sends altered value back to A;
- Once – only one change (B to A), and then they are divorced.

Your class containing a property has to support **INotifyPropertyChanged** interface or must contain an event named **{YourPropertyNamed}Changed**.
If you are too lazy to write all that boilerplate code (as me), I created a **PropertyWrapper<T>** class for you, so you can just wrap all your lovely little pieces of data in that thing. So the binding process looks like:
```c#
PropertyWrapper<float> PropertyA = new PropertyWrapper<float>();
PropertyWrapper<float> PropertyB = new PropertyWrapper<float>(4.815162342f);

void Bind()
{
	IBinding binding = Binder.Side(PropertyA).To(PropertyB).Using(BindingFlow.TwoWay);
	binding.Invalidate(BindingSide.B);
	binding.Subscribe();
}
```
Oh, one more thing. Setting **null** value to one of the sides will throw an exception. It’s a strange behaviour so I have to investigate more in this topic.

Good luck. And don’t forget to look on [Wikipedia](https://en.wikipedia.org/wiki/Ariadne) for who is Ariadne if you don’t know still.
____
# Ariadne.Framework
Ariadne.Framework is a simple UI-framework which is aimed to create small or middle-sized apps. It’s based on MVVM pattern but allows you to couple Model and ViewModel together, so you can focus on logic and representation rather than data flow. You can find an usage example here in [Mazeraptor]() repository.

It's currently in early stage of development so you have to define bindings and contexts manually. Which means writing a boilerplate code. So it got a lot of planned features.
- [ ] Automatic properties binding. You just place an attribute on which flow type this property uses.
- [ ] Automatic child views creation using fabrics and binding.
- [ ] More clear parent–children relationships.
