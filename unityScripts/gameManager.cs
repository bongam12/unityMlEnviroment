using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class gameManager : MonoBehaviour
{
    [Header("References")]
    public carControl controller;
    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    //chance population will be randomized
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    //picking 8 of best cars and 3 of worst cars
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    //number of pop to take weights
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;

    private reinforcementNet[] population;
    //low generations for performance is better algo
    [Header("Public View")]
    //debugging purposes
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        //car population for first gene pool
        population = new reinforcementNet[initialPopulation];
        //fill population with random values
        FillPopulationWithRandomValues(population, 0);
        //reset current the current car of the population
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }
    //later set starting index to end because all nets up to that point have been
    //created with other nets
    private void FillPopulationWithRandomValues(reinforcementNet[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new reinforcementNet();
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }
    //this helps improve the performance of current genome
    public void Death(float fitness, reinforcementNet network)
    {

        if (currentGenome < population.Length - 1)
        {
            //set the previous fitness from the death of the car controller
            //fitness then stored in dec order
            population[currentGenome].fitness = fitness;
            currentGenome++;
            //reset car with different net but saved fitness
            ResetToCurrentGenome();

        }
        else
        {
            //repopulate current generation
            RePopulate();
        }

    }

    //mor management oriented
    private void RePopulate()
    {
        //clear generation
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        //sort popluation by fitness
        SortPopulation();
        //also picking 3 of worst
        reinforcementNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);
        //start at naturally selected instead of zero and go through all
        //nets that havent been initialized
        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        currentGenome = 0;

        ResetToCurrentGenome();

    }

    private void Mutate(reinforcementNet[] newPopulation)
    {

        for (int i = 0; i < naturallySelected; i++)
        {

            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                //mutate a % of populations weights --> randomizes its matrix
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }

            }

        }

    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        //do not mutate all of the weights mutate some of the weights
        //pick random amount of values in matrix
        // /7 seems to work well
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            //change by random position column and row 
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);
            //instead of setting new value bump it up or down by a bit
            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;

    }

    private void Crossover(reinforcementNet[] newPopulation)
    {
        //cross over based on gene pool
        //increment by two
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            //first parent index
            int AIndex = i;
            //second parent
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    
                    //find random elemnt in gene pool
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];
                    //ensure parents are not the same
                    //we found what we looking for break
                    if (AIndex != BIndex)
                        break;
                }
            }

            reinforcementNet Child1 = new reinforcementNet();
            reinforcementNet Child2 = new reinforcementNet();

            Child1.Initialise(controller.LAYERS, controller.NEURONS);
            Child2.Initialise(controller.LAYERS, controller.NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;

            //should be more subtle in practice to much transfer of info
            for (int w = 0; w < Child1.weights.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    //taje weights from parents
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    //random chance for kids
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }

            }

            //indv values instead of matices
            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }

            }
            //add to new population
            newPopulation[naturallySelected] = Child1;
            naturallySelected++;
            //add to new population
            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private reinforcementNet[] PickBestPopulation()
    {

        reinforcementNet[] newPopulation = new reinforcementNet[initialPopulation];
        //diverse gene pool to make higher chance of top network over lower network
        for (int i = 0; i < bestAgentSelection; i++)
        {
            //copy is there because without ypu would be changine the refernce population i
            //passing over the best networks
            //increments number of filled nets
            //being added to new pop
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            //the remaining non selected have rabdomized values
            naturallySelected++;
            //take fitness, multiply by ten and add to gene pool
            //ie survival of the fittest
            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                //adding indexes to reference neural net
                //example newpopulation[genepool[i]] == neural net
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            //loop from bottom of array
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 8);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;

    }

    private void SortPopulation()
    {
        //i might rework this
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    //order from high to low fitness
                    reinforcementNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }

    }
}
