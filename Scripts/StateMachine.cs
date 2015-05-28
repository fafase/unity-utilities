using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class StateMachine: MonoBehaviour
{  
	
	#region State class declaration
	
	class StateMap
	{
		public Enum name = null;
		public List<Enum> transitions = null;
		public bool allowAnyTransition = false;
		
		public Action updateMethod = null;
		public Action fixedUpdateMethod = null;
		public Action lateUpdate = null;
		public Action <Enum> enterMethod = (state)=>{};
		public Action <Enum> exitMethod = (state)=>{};
		
		public StateMap(Enum name) 
		{
			this.name = name;
			this.allowAnyTransition = false;
			this.transitions = new List<Enum>();
		}
	}
	
	#endregion
	
	#region Variables	
	
	private Dictionary<Enum, StateMap> states;
	private Queue<Enum> transitionQueue = null;
	
	private StateMap currentState = null;
	private StateMap transitionSource = null;
	private StateMap transitionTarget = null;
	
	private bool inTransition = false;
	private bool allowMultiTransition = false;
	private bool inExitMethod = false;
	private bool initialized = false;
	private bool debugMode = false;
	
	private Action OnUpdate = null;
	private Action OnFixedUpdate = null;
	private Action OnLateUpdate = null;
	
	public Enum CurrentState { get { return this.currentState.name; } }
	
	#endregion
	
	#region Unity lifecycle
	
	protected virtual void Update() 
	{
		this.OnUpdate();
	}
	protected virtual void LateUpdate() 
	{
		this.OnLateUpdate();
	}
	protected virtual void FixedUpdate() 
	{
		this.OnFixedUpdate();
	}
	
	#endregion
	
	private bool Initialized()
	{
		if (!initialized) 
		{
			Debug.LogError( this.GetType().ToString() + ": StateMachine is not initialized. You need to call InitializeStateMachine( bool debug, bool allowMultiTransition = false )");
			return false;            
		}
		return true;
	}
	
	private static T GetMethodInfoInheritance<T>(object obj, Type type, string method, T Default) where T : class
	{
		Type baseType = type;
		while ((baseType != typeof(StateMachine)))
		{
			MethodInfo methodInfo = baseType.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (methodInfo != null)
			{
				return Delegate.CreateDelegate(typeof(T),obj, methodInfo) as T;
			}
			baseType = baseType.BaseType;
		}
		return Default;
	}
	
	/// <summary>
	/// Initializes the state machine. This has to be called from the inheriting class for the state machine to work
	/// </summary>
	/// <param name="debug">If set to <c>true</c> debug.</param>
	/// <param name="allowMultiTransition">If set to <c>true</c> allow multi transition.</param>
	protected void InitializeStateMachineWithTransitions(bool debug, bool allowMultiTransition = false ) 
	{
		if (this.initialized == true) 
		{
			Debug.LogError("The StateMachine component on " + this.GetType().ToString() + " is already initialized." );
			return;
		}
		
		this.initialized = true;
		this.states = new Dictionary<Enum, StateMap>();
		this.transitionQueue = new Queue<Enum>();
		
		// Create dummy initial state
		StateMap initial = new StateMap(Initial.InitialStateMachine); 
		initial.allowAnyTransition = true;	
		this.currentState = initial;
		this.states.Add (Initial.InitialStateMachine, initial);
		
		this.inTransition = false;
		this.transitionSource = null;
		this.transitionTarget = null;
		this.debugMode = debug;
		this.allowMultiTransition = allowMultiTransition;
	}
	
	
	protected void InitializeStateMachine<T>(bool debug, bool allowMultiTransition = false ) 
	{
		if (this.initialized == true) 
		{
			Debug.LogError("The StateMachine component on " + this.GetType().ToString() + " is already initialized." );
			return;
		}
		this.initialized = true;
		
		var values = Enum.GetValues (typeof(T));
		
		this.states = new Dictionary<Enum, StateMap>();
		this.transitionQueue = new Queue<Enum>();
		
		for (int i = 0; i < values.Length; i++)
		{
			this.initialized = this.CreateNewState((Enum)values.GetValue(i));
			StateMap stateMap = states[(Enum)values.GetValue(i)];
			stateMap.allowAnyTransition = true;
			if(i == 0)
			{
				this.currentState = stateMap;
			}
		}
		
		this.inTransition = false;
		this.transitionSource = null;
		this.transitionTarget = null;
		this.debugMode = debug;
		this.allowMultiTransition = allowMultiTransition;
		
		this.currentState.enterMethod(currentState.name);
		
		this.OnUpdate = this.currentState.updateMethod;
		this.OnFixedUpdate = this.currentState.fixedUpdateMethod;
		this.OnLateUpdate = this.currentState.lateUpdate;
	}
	
	
	/// <summary>
	/// Creates a new state.
	/// </summary>
	/// <returns><c>true</c>, if new state is successfully created, <c>false</c> otherwise.</returns>
	/// <param name="newstate">The new state to add.</param>
	protected bool CreateNewState(Enum newstate)
	{
		if (this.Initialized() == false) { return false; }
		
		if (this.states.ContainsKey(newstate) == true)
		{
			Debug.LogError("State: " + newstate + " is already registered in " + this.GetType().ToString());
			return false;
		}
		
		StateMap s = new StateMap(newstate);
		Type type = this.GetType();
		s.enterMethod =         StateMachine.GetMethodInfoInheritance<Action<Enum>>(this, type, "Enter" + newstate, DoNothingEnterExit);
		s.updateMethod =        StateMachine.GetMethodInfoInheritance<Action>(this, type, "Update" + newstate, DoNothingUpdate);
		s.fixedUpdateMethod =   StateMachine.GetMethodInfoInheritance<Action>(this, type, "FixedUpdate" + newstate, DoNothingUpdate);
		s.lateUpdate =          StateMachine.GetMethodInfoInheritance<Action>(this, type, "LateUpdate" + newstate, DoNothingUpdate);
		s.exitMethod =          StateMachine.GetMethodInfoInheritance<Action<Enum>>(this, type,"Exit" + newstate, DoNothingEnterExit);
		
		this.states.Add(newstate, s);
		
		return true;
	}
	
	/// <summary>
	/// Creates a new state with transitions.
	/// </summary>
	/// <param name="newState">The new state to add.</param>
	/// <param name="transitions">Array of allowed transitions for the new state.</param>
	protected bool CreateNewStateWithTransitions(Enum newState, Enum[] transitions)
	{
		if (this.Initialized() == false) { return false; }
		if (this.CreateNewState(newState) == false) { return false; }
		
		StateMap s = states[newState];
		
		foreach (Enum t in transitions)
		{
			if (s.transitions.Contains(t) == true)
			{
				Debug.LogError("State: " + newState + " already contains a transition for " + t + " in " + this.GetType().ToString());
				continue;
			}
			s.transitions.Add(t);
		}
		return true;
	}
	
	/// <summary>
	/// Determines whether the transition between fromstate to tostate is defined as a legal transition
	/// </summary>
	/// <returns><c>true</c> if this instance is legal transition the specified fromstate tostate; otherwise, <c>false</c>.</returns>
	/// <param name="fromstate">Fromstate.</param>
	/// <param name="tostate">Tostate.</param>
	protected bool IsLegalTransition(Enum fromstate, Enum tostate) 
	{
		if (this.Initialized() == false) { return false; }
		
		if (this.states.ContainsKey(fromstate) && this.states.ContainsKey(tostate)) 
		{
			if (this.states[fromstate].allowAnyTransition || this.states[fromstate].transitions.Contains(tostate)) 
			{
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Changes the current state.
	/// </summary>
	/// <returns><c>true</c>, if the transition to the new state is legal, <c>false</c> otherwise.</returns>
	/// <param name="newstate">Newstate.</param>
	protected bool ChangeCurrentState(Enum newstate) 
	{
		if (this.Initialized() == false) { return false;}
		
		if (this.inTransition) 
		{
			if (this.allowMultiTransition == false)
			{
				if(this.debugMode == true)
				{
					Debug.LogWarning(this.GetType().ToString() + " requests transition to state " + newstate +
					                 " when still transitioning to state " + this.transitionTarget.name);
				} 
				
				return false;
			}
			else if (this.allowMultiTransition == true) 
			{
				if (this.inExitMethod == true) 
				{
					Debug.LogWarning(this.GetType().ToString() + " requests new state in exit method is not recommended");
					return false;
				}
				this.transitionQueue.Enqueue(newstate);
				return true;
			}
		}
		
		if( this.IsLegalTransition( this.currentState.name, newstate )) 
		{
			if (this.debugMode == true) 
			{
				Debug.Log( this.GetType().ToString() + " transition: " + this.currentState.name + " => " + newstate );
			}
			
			this.transitionSource = this.currentState;
			this.transitionTarget = this.states[newstate];
			this.inTransition = true;
			this.inExitMethod = true;
			this.currentState.exitMethod(this.transitionTarget.name);
			this.inExitMethod = false;
			this.transitionTarget.enterMethod(this.currentState.name);
			this.currentState = this.transitionTarget;
			
			if (this.transitionTarget == null || this.transitionSource == null)
			{
				Debug.LogError(this.GetType().ToString() + " cannot finalize transition; source or target state is null!");
				
			}
			else 
			{
				this.inTransition = false;
				this.transitionSource = null;
				this.transitionTarget = null;
			}  
		} 
		else 
		{
			Debug.LogError( this.GetType().ToString() + " requests transition: " + this.currentState.name + " => " + newstate + " is not a defined transition!" );
			return false;
		}
		if (this.allowMultiTransition == true && this.transitionQueue.Count != 0) 
		{
			this.ChangeCurrentState(this.transitionQueue.Dequeue());
		}
		
		this.OnUpdate = 		this.currentState.updateMethod;
		this.OnFixedUpdate = 	this.currentState.fixedUpdateMethod;
		this.OnLateUpdate = 	this.currentState.lateUpdate;
		return true;
	}
	
	private static void DoNothingUpdate(){}
	private static void DoNothingEnterExit(Enum state){} 
	protected enum Initial{ InitialStateMachine }
}
