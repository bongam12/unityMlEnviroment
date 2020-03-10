using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class gameManager : MonoBehaviour
{
    [Header("References")]
    public carControl controller;
    public carControl2 player2;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;

    private reinforcementNet[] population;
    private reinforcement2Child[] population2;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;
    public int currentGeneration2;
    public int currentGenome2 = 0;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new reinforcementNet[initialPopulation];
        population2 = new reinforcement2Child[initialPopulation];
        FillPopulationWithRandomValues(population,population2, 0);
        ResetToCurrentGenome();

        //second person
        
        
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
        player2.ResetWithNetwork(population2[currentGenome2]);
    }

    private void FillPopulationWithRandomValues(reinforcementNet[] newPopulation, reinforcement2Child[] newPopulation2, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new reinforcementNet();
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS);
            //second person
            newPopulation2[startingIndex] = new reinforcement2Child();
            newPopulation2[startingIndex].Initialise2(player2.LAYERS, player2.NEURONS);
            startingIndex++;
        }
    }

    public void Death(float fitness, reinforcementNet network)
    {
       

        if (currentGenome < population.Length - 1)
        {

            population[currentGenome].fitness = fitness;
            Debug.Log("death first layer");
            currentGenome++;
            Debug.Log("death active");
            ResetToCurrentGenome();
            

        }
        else
        {
            RePopulate();
        }


    }
    public void Death2(float fitness2,reinforcement2Child network2)
    {
        if (currentGenome2 < population2.Length - 1)
        {

            population2[currentGenome2].fitness = fitness2;
            Debug.Log("death first layer");
            currentGenome2++;
            Debug.Log("death active");
            ResetToCurrentGenome();


        }
        else
        {
            RePopulate();
        }
    }

    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        currentGeneration2++;
        naturallySelected = 0;
        SortPopulation();

        reinforcementNet[] newPopulation = PickBestPopulation();
        reinforcement2Child[] newPopulation2 = PickBestPopulation2();

        Crossover(newPopulation,newPopulation2);
        Mutate(newPopulation,newPopulation2);

        FillPopulationWithRandomValues(newPopulation,newPopulation2, naturallySelected);

        population = newPopulation;
        population2 = newPopulation2;

        currentGenome = 0;
        currentGenome2 = 0;

        ResetToCurrentGenome();

    }

    private void Mutate(reinforcementNet[] newPopulation, reinforcement2Child[] newPopulation2)
    {

        for (int i = 0; i < naturallySelected; i++)
        {

            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {

                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                    newPopulation2[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }

            }

        }

    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {

        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;

    }

    private void Crossover(reinforcementNet[] newPopulation, reinforcement2Child[] newPopulation2)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            reinforcementNet Child1 = new reinforcementNet();
            reinforcementNet Child2 = new reinforcementNet();
            //
            reinforcement2Child Child1_A = new reinforcement2Child();
            reinforcement2Child Child2_A = new reinforcement2Child();

            Child1.Initialise(controller.LAYERS, controller.NEURONS);
            Child2.Initialise(controller.LAYERS, controller.NEURONS);

            Child1_A.Initialise2(player2.LAYERS, player2.NEURONS);
            Child2_A.Initialise2(player2.LAYERS, player2.NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;

            Child1_A.fitness = 0;
            Child2_A.fitness = 0;


            for (int w = 0; w < Child1.weights.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                    //second person
                    Child2_A.weights[w] = population2[AIndex].weights[w];
                    Child1_A.weights[w] = population2[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                    //second person
                    Child1_A.weights[w] = population2[AIndex].weights[w];
                    Child2_A.weights[w] = population2[BIndex].weights[w];
                }

            }


            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                    //second person
                    Child2_A.biases[w] = population2[AIndex].biases[w];
                    Child1_A.biases[w] = population2[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                    //second person
                    Child1_A.biases[w] = population2[AIndex].biases[w];
                    Child2_A.biases[w] = population2[BIndex].biases[w];
                }

            }

            newPopulation[naturallySelected] = Child1;
            newPopulation2[naturallySelected] = Child1_A;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            newPopulation2[naturallySelected] = Child2_A;
            naturallySelected++;

            

        }
    }

    private reinforcementNet[] PickBestPopulation()
    {

        reinforcementNet[] newPopulation = new reinforcementNet[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;

    }

    private reinforcement2Child[] PickBestPopulation2()
    {

        reinforcement2Child[] newPopulation2 = new reinforcement2Child[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation2[naturallySelected] = population2[i].InitialiseCopy2(player2.LAYERS, player2.NEURONS);
               

            newPopulation2[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation2;

    }

    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    reinforcementNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
                if (population2[i].fitness < population2[j].fitness)
                {
                    reinforcement2Child temp2 = population2[i];
                    population2[i] = population2[j];
                    population2[j] = temp2;
                }
            }
        }

    }
}
