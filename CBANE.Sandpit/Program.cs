using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

using CBANE.Core;
using static CBANE.Core.NEMath;

namespace CBANE.Sandpit
{
    public enum ProgramStatus
    {
        UNKNOWN,
        IDLE,
        TRAINING,
        TESTING,
        EXITING
    }

    class Program
    { 
        static bool running = true;

        static ExampleTrainer Trainer;

        static ProgramStatus outputStatus = ProgramStatus.UNKNOWN;

        static void Main(string[] args)
        {
            var superclusterConfig = new SuperclusterConfig(5);
            var clusterConfig = new ClusterConfig(50, 0.20, 0.05);
            var networkConfig = new NetworkConfig(3, 2, 3, 12);

            networkConfig.InputActivation = ActivationTypes.Passthrough;
            networkConfig.OutputActivation = ActivationTypes.Softmax;
            networkConfig.HiddenActivation = ActivationTypes.LeakyReLU;

            var supercluster = new Supercluster(superclusterConfig, clusterConfig, networkConfig);

            supercluster.GenerateRandomPopulation();

            Trainer = new ExampleTrainer(supercluster, 25, 50, 15);

            outputStatus = ProgramStatus.IDLE;

            var displayTask = Task.Run(() => ManageDisplay());

            Task trainingTask = null;

            while(running)
            {
                var keyInfo = Console.ReadKey(true);

                if(outputStatus == ProgramStatus.IDLE && keyInfo.Key == ConsoleKey.R)
                {
                    outputStatus = ProgramStatus.TRAINING;
                    trainingTask = Task.Run(() => PerformTrainingRun());
                }
                else if(keyInfo.Key == ConsoleKey.X)
                {
                    outputStatus = ProgramStatus.EXITING;

                    if(trainingTask != null && !trainingTask.IsCompleted)
                    {
                        Trainer.StopTraining();

                        trainingTask.Wait();
                    }
                    
                    running = false;
                    displayTask.Wait();
                }
            }
        }

        static void PerformTrainingRun()
        {
            Trainer.Train(500);

            if(outputStatus != ProgramStatus.EXITING)
            {
                outputStatus = ProgramStatus.TESTING;

                Trainer.Evaluate(EvaluationMode.TESTING, true);

                outputStatus = ProgramStatus.IDLE;
            }
        }

        static void ManageDisplay()
        {
            ResetDisplay();

            var currentStatus = ProgramStatus.UNKNOWN;
            var currentProgress = 0;

            while(running)
            {
                if(Console.WindowTop != 0)
                    ResetDisplay();

                var statusMessage = " " + outputStatus.ToString().ToUpper();

                // Update Status Indicator
                var statusFGColour = ConsoleColor.Black;
                var statusBGColour = ConsoleColor.Gray;

                switch(outputStatus)
                {
                    case ProgramStatus.TRAINING:
                        statusFGColour = ConsoleColor.Black;
                        statusBGColour = ConsoleColor.Cyan;
                        break;

                    case ProgramStatus.TESTING:
                        statusFGColour = ConsoleColor.Black;
                        statusBGColour = ConsoleColor.Green;
                        break;

                    case ProgramStatus.EXITING:
                        statusFGColour = ConsoleColor.Black;
                        statusBGColour = ConsoleColor.Yellow;
                        break;

                    case ProgramStatus.IDLE:
                    default:
                        statusFGColour = ConsoleColor.Black;
                        statusBGColour = ConsoleColor.Gray;
                        break;
                }

                if(currentStatus != outputStatus)
                {
                    currentStatus = outputStatus;
                    currentProgress = 0;
                }

                WriteAt(statusMessage.PadRight(Console.WindowWidth), 1, 0, statusFGColour, statusBGColour);

                if(currentStatus != ProgramStatus.IDLE)
                {
                    var progressLeft = statusMessage.Length;

                    WriteAt((currentProgress > 0) ? "." : " ", 1, progressLeft, statusFGColour, statusBGColour);
                    WriteAt((currentProgress > 1) ? "." : " ", 1, progressLeft + 1, statusFGColour, statusBGColour);
                    WriteAt((currentProgress > 2) ? "." : " ", 1, progressLeft + 2, statusFGColour, statusBGColour);

                    currentProgress = (currentProgress + 1 > 3) ? 0 : currentProgress + 1;
                }

                // Update Scores
                WriteAt(" BEST SCORES:".PadRight(Console.WindowWidth), 3, 0);

                string trainingScore = (Trainer.BestTrainingScore == double.MinValue) ? "N/A" : $"{Trainer.BestTrainingScore / Trainer.MaxTrainingScore * 100:0.000000}%";
                string trainingScoreMessage = $" > TRAINING: {trainingScore}";

                string testingScore = (Trainer.BestTestingScore == double.MinValue) ? "N/A" : $"{Trainer.BestTestingScore / Trainer.MaxTestingScore * 100:0.000000}%";
                string testingScoreMessage =  $" > TESTING:  {testingScore}";

                WriteAt(trainingScoreMessage.PadRight(Console.WindowWidth), 4, 0);
                WriteAt(testingScoreMessage.PadRight(Console.WindowWidth), 5, 0);

                // Update Stats
                WriteAt(" STATISTICS:".PadRight(Console.WindowWidth), 7, 0);

                string cyclesMessage =   $" > CYCLES:   {Trainer.Cycle}";
                string clustersMessage = $" > CLUSTERS: {Trainer.Supercluster.Clusters.Count}";

                WriteAt(cyclesMessage.PadRight(Console.WindowWidth), 8, 0);
                WriteAt(clustersMessage.PadRight(Console.WindowWidth), 9, 0);

                // Display Menu
                WriteAt(" OPTIONS:".PadRight(Console.WindowWidth), 11, 0);

                if(currentStatus == ProgramStatus.IDLE)
                {
                    WriteAt(" > [R] Run Training (500 cycles)".PadRight(Console.WindowWidth), 12, 0);
                    WriteAt(" > [X] Exit".PadRight(Console.WindowWidth), 13, 0);
                }
                else
                {
                    WriteAt(" > [X] Exit".PadRight(Console.WindowWidth), 12, 0);
                    WriteAt("".PadRight(Console.WindowWidth), 13, 0);
                }

                Thread.Sleep(250);
            }
        }

        static void ResetDisplay()
        {
            Console.Clear();
            Console.CursorVisible = false;

            string title = " CBANE Sandpit";

            WriteAt(title.PadRight(Console.WindowWidth), 0, 0, ConsoleColor.White, ConsoleColor.DarkRed);
        }

        static void WriteAt(string message, int top, int left, ConsoleColor fgColour = ConsoleColor.White, ConsoleColor bgColour = ConsoleColor.Black)
        {
            Console.CursorTop = top;
            Console.CursorLeft = left;

            Console.ForegroundColor = fgColour;
            Console.BackgroundColor = bgColour;

            Console.Write(message);
        }

    }
}
