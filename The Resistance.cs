using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Museum
{
    class Morse
    {
        private readonly Dictionary<char, string> lettersToMorse = new Dictionary<char, string>()
                                                                        {
                                                                            { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." }, { 'E', "." }, { 'F', "..-." }, 
                                                                            { 'G', "--." }, { 'H', "...." }, { 'I', ".." }, { 'J', ".---" }, { 'K', "-.-" }, { 'L', ".-.." }, 
                                                                            { 'M', "--" }, { 'N', "-." }, { 'O', "---" }, { 'P', ".--." }, { 'Q', "--.-" }, { 'R', ".-." }, 
                                                                            { 'S', "..." }, { 'T', "-" }, { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" }, 
                                                                            { 'Y', "-.--" }, { 'Z', "--.." }
                                                                        };

        private string morseSequence;
        private HashSet<string> allowedWords;

        public void ReadFromConsole()
        {
            Program.WriteLineLocally("Please enter the Morse sequence: ");
            this.morseSequence = Console.ReadLine();

            Program.WriteLineLocally("How many words are there in the dictionary? ");
            int dictionarySize = int.Parse(Console.ReadLine());

            this.allowedWords = new HashSet<string>();

            Program.WriteLineLocally("Please enter, line by line, all the words in the dictionary: ");
            for (int count = 0; count < dictionarySize; count++)
            {
                string word = Console.ReadLine();
                this.allowedWords.Add(word);
            }
        }

        public void ReadFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            this.morseSequence = lines.First();

            int dictionarySize = int.Parse(lines[1]);
            this.allowedWords = new HashSet<string>();

            for (int line = 2; line < lines.Length; line++)
            {
                string word = lines[line];
                this.allowedWords.Add(word);
            }
        }

        public long CalculateNumPossibleMatches()
        {
            Dictionary<string, int> numWordMatchesByMorseString = createDictOfNumWordsWithSameMorseCode();

            int longestMorseCodeOfASingleWord = numWordMatchesByMorseString.Keys.Max(key => key.Length);

            long[] numMatchesByEachPosition = new long[this.morseSequence.Length];

            for (int inclusiveWordEndIndex = 0; inclusiveWordEndIndex < this.morseSequence.Length; inclusiveWordEndIndex++)
            {
                //If the value of i is less than the length of the longest Morse code of a single English word, then it is possible that the Morse snippet from the beginning of the Morse sequence to i
                //corresponds to just one English word. Otherwise, there are >1 English words encoded in the Morse message.
                for (int wordStartIndex = Math.Max(0, inclusiveWordEndIndex - longestMorseCodeOfASingleWord + 1); wordStartIndex <= inclusiveWordEndIndex; wordStartIndex++)
                {
                    int wordLength = inclusiveWordEndIndex - wordStartIndex + 1;
                    string morseSubstring = morseSequence.Substring(wordStartIndex, wordLength);

                    int numWordsMatchingMorseSubstring;
                    bool oneOrMoreMatchesFound = numWordMatchesByMorseString.TryGetValue(morseSubstring, out numWordsMatchingMorseSubstring);

                    if (oneOrMoreMatchesFound)
                    {
                        if (wordStartIndex == 0)
                        {
                            //I.e., if we are currently treating the Morse sequence (up to index i) as encoding only one English word...
                            numMatchesByEachPosition[inclusiveWordEndIndex] = numWordsMatchingMorseSubstring;
                        }
                        else
                        {
                            //If the Morse message encodes, say, two words, then we need to multiply the number of words that share the same Morse code as the second word with the number of words that share
                            //the same Morse code in the first word. 

                            //In general, we need to multiply the number of words that share the same Morse code as the current word with the number of matches that are compatible with the Morse code that comes
                            //before the Morse code for the current word begins.
                            numMatchesByEachPosition[inclusiveWordEndIndex] += numWordsMatchingMorseSubstring * numMatchesByEachPosition[wordStartIndex - 1];
                        }
                    }
                }
            }

            return numMatchesByEachPosition.Last();
        }

        private Dictionary<string, int> createDictOfNumWordsWithSameMorseCode()
        {
            var numAllowedWordsByMorseString = new Dictionary<string, int>();

            IEnumerable<string> morseTranslations = this.allowedWords.Select(translateWordToMorse);

            foreach (string morseTranslation in morseTranslations)
            {
                if (numAllowedWordsByMorseString.ContainsKey(morseTranslation))
                {
                    numAllowedWordsByMorseString[morseTranslation] += 1;
                }
                else
                {
                    numAllowedWordsByMorseString[morseTranslation] = 1;                    
                }
            }

            return numAllowedWordsByMorseString;
        }

        private string translateWordToMorse(string word)
        {
            StringBuilder morse = new StringBuilder();

            foreach (char character in word)
            {
                string morseCode = this.lettersToMorse[character];
                morse.Append(morseCode);
            }

            return morse.ToString();
        }
    }

    class Program
    {
        private static bool isRunningLocally;

        internal static void WriteLineLocally(string input)
        {
            if (isRunningLocally)
            {
                Console.WriteLine(input);
             }
         }

        static void Main(string[] args)
        {
            isRunningLocally = args.Length > 0;

            var morse = new Morse();

            morse.ReadFromConsole();
            long numMatches = morse.CalculateNumPossibleMatches();

            Console.WriteLine(numMatches.ToString());

            Console.WriteLine("Press any key to exit: ");
            Console.ReadKey();
         }
     }
 }
