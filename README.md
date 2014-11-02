unity-utilities
===============

Using StateMachine script:

Any class requiring a state machine simply needs to inherits from StateMachine.
Then it is required to initialize the StateMachine, declare the states and if necessary declare the legal transitions
Finally, the starting state is requested.


	public MyClass:StateMachine
	{
	    void Start(){
			// Parameter will print out information on transitions
			Initialize(true);
			// Add each needed state with the legal transitions.
			// Use AddState("Play"); if all legal transitions
			AddStateWithTransitions("Play", new string[] { "Pause", "Dead" });
			AddStateWithTransitions("Pause", new string[] { "Pause"});
			AddStateWithTransitions("Dead", new string[] { "Play" });
			// Define a state
			RequestState("Play"); 
		}
	}

For each state it is possible to use Enter/Update/ExitXXX, the last part being the name of the state.

    public MyClass:StateMachine
	{
		void Update()
		{
			// This will call the current state update method if any
			StateUpdate();
		}
	    protected virtual void EnterPlay(string sourceState)
		{}
			
		protected virtual void UpdatePlay()
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				RequestState("Pause");
			}
		}
		
		protected virtual void ExitPlay(string nextState)
		{}
	}

Each method is not compulsory, only needed methods should be declared.
If a state is requested but is not legal, the StateMachine will print a debug error and won't transit.

