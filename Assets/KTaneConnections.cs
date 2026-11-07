using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class KTaneConnections : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	
	public KMSelectable[] buttons;
	
	public Material[] materials;
	public Material[] buttonStates;

    private static int moduleCount;
    private int moduleId;
	private bool isSolved;
	
	private BoardResult GameBoard;
	
	private int rowsCorrect = 0;
	private List<string> answerCheck = new List<string>();
	private List<KMSelectable> selectedButtons = new List<KMSelectable>();
	
	public const int size = 4;

    private static readonly Dictionary<string, List<string>> TEXT_OPTIONS =
        new Dictionary<string, List<string>>
        {
            { "bosses", new List<string>
                { "Spectator Sport", "Forget Me Not", "Blackout", "8", "Duck Konundrum",
                  "Ubermodule", "Souvenir", "Simon's Stages", "Forget Them All", "Forget Maze Not" } },

            { "vanilla", new List<string>
                { "The Button", "Knob", "Keypad", "Memory", "Password", "Who's on First",
                  "Venting Gas", "Wires", "Complicated Wires", "Morse Code", "Maze" } },

            { "needies", new List<string>
                { "Knob", "Venting Gas", "Injector", "Refill that Beer!", "Pou",
                  "Metronome", "Blinker", "Not Not Not", "Counting Sheep",
                  "Morse Identification", "Swapsies", "Office Job" } },

            { "causes instant death", new List<string>
                { "Codenames", "Ultimate Custom Night", "Lunchtime", "Turn The Key",
                  "Sword of Damocles", "Counting Sheep", "Dancing Triangle", "BadTV",
                  "Buddy Bidding" } },

            { "red modules", new List<string>
                { "Morsematics", "Not Morsematics", "Forget Me Not", "Cruel Simpleton",
                  "Cruel Keypads", "Crazy Food", "Suffering Maze", "Unfair's Revenge",
                  "Password Destroyer", "Chicken Nuggets" } },

            { "wire modules", new List<string>
                { "Wires", "Cutting Order", "Sequential Button", "Forget Them All",
                  "Binary LEDs", "Curly Wires", "Complicated Wires" } },

            { "solos", new List<string>
                { "Battleship", "Luigi Poker", "3D Lights Out", "Addition", "perspective eggs",
                  "8", "501", "Anagrams", "Winning Line", "Catch Me if You Can", "Swapsies", "UNO!",
				  "Venting Gas", "Refill that Beer!", "Counting Sheep", "Office Job", "Blinker",
				  "Pou" } },

            { "card modules", new List<string>
                { "Golf Solitaire", "Luigi Poker", "Stacked Deck", "UNO!", "Badugi",
                  "Baccarat", "Codenames", "Blackjack", "Poker" } },

            { "morse modules", new List<string>
                { "Sky Plate", "Morsematics", "Ubermodule", "Morse Code", "Not Morse Code",
                  "Goofy's Game", "Infinite Loop", "Semamorse", "Morse Buttons",
                  "Basic Morse", "Reverse Morse" } },

            { "based on games", new List<string>
                { "Phasmophobia", "Luigi Poker", "8", "UNO!", "Codenames",
                  "Ultimate Custom Night", "Puppet", "Withered Foxy", "Spectator Sport",
                  "BadTV", "Mega Man 2", "Book of Mario", "Not Chess", "Golf Solitaire",
				  "Badugi", "Baccarat", "Blackjack", "Poker", "Chess"} },

            { "cipher modules", new List<string>
                { "Long Words", "BorisP", "Copper-9", "Unfair's Revenge", "Basic Morse",
                  "Yellow Cipher", "Caesar Cycle", "Caesar Cipher", "Semaphore", "Reverse Morse",
                  "Stack'em", "OmegaDestroyer" } },

            { "time dependent", new List<string>
                { "Password Destroyer", "Turn The Key", "The Time Keeper", "Timing is Everything",
                  "OmegaDestroyer", "Doomsday Button", "n-Time", "Again" } },
				  
			{ "navigation modules", new List<string>
				{ "Suffering Maze", "Forget Maze Not", "Sky Plate", "Not Orientation Cube", "Maze", 
				  "Navigation Determination", "Not Maze", "Echolocation" } },
			
			{ "not modules", new List<string>
				{ "Forget Maze Not", "Not Morse Code", "Not Morsematics", "Forget Me Not", "Not Not Not",
				  "Not Chess", "Not Orientation Cube", "Not Maze" } },
				  
			{ "grid modules", new List<string>
				{ "Swapsies", "Not Maze", "Maze", "Battleship", "3D Lights Out", "Winning Line", "Suits and Colours",
				  "Mega Man 2" } },
				  
			{ "single button modules", new List<string>
				{ "Puppet", "Power Button", "8", "The Button", "Refill that Beer!", "Sword of Damocles", 
				  "Morsematics", "Not Morsematics", "Timing is Everything", "Doomsday Button", "Again" } },
				  
		    { "non-default backgrounds", new List<string>
			    { "Spectator Sport", "Blackout", "Simon's Stages", "Forget Them All", "Codenames", 
				  "Ultimate Custom Night", "Lunchtime", "BadTV", "Buddy Bidding", "Morsematics", "Not Morsematics",
				  "Forget Me Not", "Cruel Simpleton", "Cruel Keypads", "Crazy Food", "Suffering Maze", 
				  "Unfair's Revenge", "Password Destroyer", "Chicken Nuggets", "Luigi Poker", "perspective eggs", 
				  "Winning Line", "Catch Me if You Can", "Swapsies", "UNO!", "Bumpers", "Office Job", "Suits and Colours", 
				  "Golf Solitaire", "Badugi", "Baccarat", "Blackjack", "Goofy's Game", "Semamorse", "Phasmophobia", 
				  "Long Words", "Copper-9", "Yellow Cipher", "Stack'em", "The Time Keeper", "Doomsday Button", "Again", 
				  "Echolocation" } },
						
        };

    void Awake() {
        moduleId = moduleCount++;
		
        foreach (KMSelectable button in buttons) {
			StartCoroutine(UpdateButton(button));

            button.OnInteract += delegate () { 
				var index = Array.IndexOf(buttons, button);
				button.AddInteractionPunch();
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
				SelectButton(index);
			return false; };
        }
    }
	
	void Start () {
	
		GameBoard = GenerateBoard();
        var arr = GameBoard.Board;
        var solutions = GameBoard.Solutions;

        for (int r = 0; r < 4; r++) {
            string row = "";
            for (int c = 0; c < 4; c++)
                row += " " + arr[r, c];
			Debug.LogFormat("[KTane Connections #{0}] Row {1}:{2}", moduleId, r, row);
        }

        foreach (var sol in solutions) {
			Debug.LogFormat("[KTane Connections #{0}] Set {1}: {2}", moduleId, sol.Key, string.Join(", ", sol.Labels));
        }
	
	}
    IEnumerator UpdateButton(KMSelectable button)
    {
        Vector3 originalPosition = button.transform.localPosition ;
        Vector3 lowestPosition = originalPosition + Vector3.down * 0.2f;

		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
        float elapsed = 0f;
		float moveDuration = 0.66f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            button.transform.localPosition  = Vector3.Lerp(
                originalPosition, lowestPosition, elapsed / moveDuration);
            yield return null;
        }

        button.transform.localPosition  = lowestPosition;
		var index = Array.IndexOf(buttons, button);
		
		Renderer icon = buttons[index].transform.Find("Icon").GetComponent<Renderer>();
		Material target = materials.FirstOrDefault(m => m.name == GameBoard.Board[index / size, index % size]);
		if (index / size < rowsCorrect || isSolved) SetButtonState(button, 1);
		else SetButtonState(button, 0);

		icon.material = target;

        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            button.transform.localPosition  = Vector3.Lerp(
                lowestPosition, originalPosition, elapsed / moveDuration);
            yield return null;
        }

        button.transform.localPosition  = originalPosition;
    }


    public struct BoardResult {
		
        public string[,] Board;
        public List<Solution> Solutions;

        public BoardResult(string[,] board, List<Solution> solutions) {
            Board = board;
            Solutions = solutions;
        }
    }

	public class Solution {
		
		public string Key { get; set; }
		public string[] Labels { get; set; }
		
	}

	public BoardResult GenerateBoard()
	{

		var options = TEXT_OPTIONS;

		var keys = Shuffle(options.Keys.ToList());

		var selectedKeys = new List<string>();
		var groups = new List<List<string>>();

		if (!FindGroups(keys, 0, selectedKeys, groups,
			new HashSet<string>(), options, size)) {
			return new BoardResult(null, new List<Solution>());
		}

		var solutions = selectedKeys.Select((key, i) => new Solution {
			Key = key,
			Labels = groups[i].ToArray()
		}).ToList();

		var labels = Shuffle(solutions.SelectMany(s => s.Labels).ToList());
		var board = new string[size, size];

		for (int i = 0; i < labels.Count; i++)
			board[i / size, i % size] = labels[i];

		return new BoardResult(board, solutions);
		
	}
	
	public void SelectButton(int index) {
		if (index / size < rowsCorrect || isSolved) return;
		string val = GameBoard.Board[index / size, index % size];

		if(answerCheck.Contains(val)) {
			answerCheck.Remove(val);
			selectedButtons.Remove(buttons[index]);
			SetButtonState(buttons[index], 0);
		} else {
			answerCheck.Add(val);
			selectedButtons.Add(buttons[index]);
			SetButtonState(buttons[index], 1);
		}
		
		if(answerCheck.Count == 4) {
			bool valid = CheckSolution(answerCheck, rowsCorrect);
			
			if (valid) {
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
				foreach (KMSelectable button in buttons) {
					StartCoroutine(UpdateButton(button));
				}
				rowsCorrect++;
				if (rowsCorrect >= 3) {
					Debug.LogFormat("[KTane Connections #{0}] All sets selected, module solved.", moduleId);
					module.HandlePass();
					isSolved = true;
				}
			}
			if (!valid) { 
				Debug.LogFormat("[KTane Connections #{0}] Selected Set: {1} is incorrect. Strike.", moduleId, string.Join(", ", answerCheck.Select(x => x.ToString()).ToArray()));
				module.HandleStrike();
				foreach (KMSelectable button in buttons) {
					if (Array.IndexOf(buttons, button) / size < rowsCorrect) continue;
					StartCoroutine(FlashButton(button));
				}
			}
			
			answerCheck.Clear();
		}
		
	}
	
    IEnumerator FlashButton(KMSelectable button)
    {
		SetButtonState(button, 2);
		yield return new WaitForSeconds(.33f);
		SetButtonState(button, 0);
    }
	
	public void SetButtonState(KMSelectable button, int state) {
		Renderer led = button.transform.Find("LED").GetComponent<Renderer>();
		Material target = buttonStates[state];
		
		led.material = target;
	}
	
	public bool CheckSolution(List<string> input, int targetRow) {
		var wantedSet = new HashSet<string>(input, StringComparer.OrdinalIgnoreCase);
		var board = GameBoard.Board;
		var solutions = GameBoard.Solutions;

		Solution matchingSolution = null;
		foreach (var sol in solutions)
		{
			var solSet = new HashSet<string>(sol.Labels, StringComparer.OrdinalIgnoreCase);
			if (solSet.SetEquals(wantedSet))
			{
				matchingSolution = sol;
				break;
			}
		}

		if (matchingSolution == null)
			return false;

		var allLabels = new List<string>(size * size);
		for (int r = 0; r < size; r++)
			for (int c = 0; c < size; c++)
				allLabels.Add(board[r, c]);

		foreach (string w in input)
			allLabels.RemoveAll(l => string.Equals(l, w, StringComparison.OrdinalIgnoreCase));

		int leftoverIndex = 0;
		for (int r = 0; r < size; r++)
		{
			for (int c = 0; c < size; c++)
			{
				if (r == targetRow)
					board[r, c] = input[c];
				else
					board[r, c] = allLabels[leftoverIndex++];
			}
		}

		return true;
	}


	private bool FindGroups( List<string> keys,	int start, List<string> selectedKeys, List<List<string>> groups,
							 HashSet<string> used, Dictionary<string, List<string>> options, int size) {
								 
		if (selectedKeys.Count == size)
			return true;

		for (int i = start; i < keys.Count; i++) {
			
			string key = keys[i];

			var available = options[key]
				.Where(label => !used.Contains(label))
				.ToList();

			if (available.Count < size)
				continue;

			Shuffle(available);

			foreach (var mod in Combinations(available, size)) {
				
				selectedKeys.Add(key);
				groups.Add(mod);
				used.UnionWith(mod);

				if (FindGroups(keys, i + 1, selectedKeys, groups,
					used, options, size))
					return true;

				selectedKeys.RemoveAt(selectedKeys.Count - 1);
				groups.RemoveAt(groups.Count - 1);

				foreach (var label in mod)
					used.Remove(label);
					
			}
		}

		return false;
	}

	private IEnumerable<List<T>> Combinations<T>(List<T> items, int count)	{
		
		if (count == 0)
		{
			yield return new List<T>();
			yield break;
		}

		for (int i = 0; i <= items.Count - count; i++) {
			var remaining = items.Skip(i + 1).ToList();

			foreach (var combination in Combinations(remaining, count - 1))
				yield return new List<T> { items[i] }
					.Concat(combination).ToList();
					
		}
	}

    private List<T> Shuffle<T>(List<T> list) {
		
        for (int i = 0; i < list.Count - 1; i++) {
            int j = UnityEngine.Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
		return list;
		
    }
}
