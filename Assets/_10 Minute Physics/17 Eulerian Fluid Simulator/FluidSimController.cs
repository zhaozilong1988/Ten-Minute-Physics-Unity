using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluidSimulator;

//Most basic fluid simulator
//Based on: "How to write an Eulerian Fluid Simulator with 200 lines of code" https://matthias-research.github.io/pages/tenMinutePhysics/
//Eulerian means we simulate the fluid in a grid - not by using particles (Lagrangian). One can also use a combination of both methods
//Can simulate both liquids and gas
//Assume incompressible fluid with zero viscosity (inviscid) which are good approximations for water and gas
public class FluidSimController : MonoBehaviour
{
    public Material fluidMaterial;

    private Scene scene;

    private DisplayFluid displayFluid;

    private FluidUI fluidUI;


    private void Start()
    {    
        scene = new Scene();

        displayFluid = new DisplayFluid(fluidMaterial);

        fluidUI = new FluidUI(this);

        //SetupScene(Scene.SceneNr.WindTunnel);

        //SetupScene(Scene.SceneNr.Tank);
    }



    private void Update()
    {
        //Display the fluid
        //Draw();

        displayFluid.TestDraw();
    }



    private void FixedUpdate()
    {
        return;
    
        //Simulate the fluid
        if (!scene.isPaused)
        {
            scene.fluid.Simulate(scene.dt, scene.gravity, scene.numIters, scene.overRelaxation);

            scene.frameNr++;
        }
    }



    //Init the simulation after a button has been pressed
    public void SetupScene(Scene.SceneNr sceneNr = Scene.SceneNr.Tank)
    {
        scene.sceneNr = sceneNr;
        scene.obstacleRadius = 0.15f;
        scene.overRelaxation = 1.9f;

        scene.dt = Time.fixedDeltaTime;
        scene.numIters = 40;

        //How detailed the simulation is in height direction
        int res = 100;

        if (sceneNr == Scene.SceneNr.Tank)
        {
            res = 50;
        }
        else if (sceneNr == Scene.SceneNr.HighResWindTunnel)
        {
            res = 200;
        }


        //The height of the simulation is 1 m (in the tutorial) but the guy is also setting simHeight = 1.1 and domainHeight = 1 so Im not sure which is which. But he says 1 m in the video
        float simHeight = 1f;

        //The size of a cell
        float h = simHeight / res;

        //How many cells do we have
        //y is up
        int numY = Mathf.FloorToInt(simHeight / h);
        //Twice as wide
        int numX = 2 * numY;

        //Density of the fluid (water)
        float density = 1000f;

        FluidSim f = scene.fluid = new FluidSim(density, numX, numY, h);

        //not same as numY above because we add a border?
        int n = f.numY;

        if (sceneNr == Scene.SceneNr.Tank)
        {           
            //Add a solid border
            for (int i = 0; i < f.numX; i++)
            {
                for (int j = 0; j < f.numY; j++)
                {
                    //Fluid
                    float s = 1f;
                    
                    if (i == 0 || i == f.numX - 1 || j == 0)
                    {
                        s = 0f;
                    }

                    f.s[i * n + j] = s;
                }
            }

            scene.gravity = -9.81f;
            scene.showPressure = true;
            scene.showSmoke = false;
            scene.showStreamlines = false;
            scene.showVelocities = false;
        }
        else if (sceneNr == Scene.SceneNr.WindTunnel || sceneNr == Scene.SceneNr.HighResWindTunnel)
        {
            //Wind velocity
            float inVel = 2f;
            
            for (int i = 0; i < f.numX; i++)
            {
                for (int j = 0; j < f.numY; j++)
                {
                    //Fluid
                    float s = 1f;

                    if (i == 0 || j == 0 || j == f.numY - 1)
                    {
                        //Solid
                        s = 0f;
                    }
                    f.s[i * n + j] = s;

                    //Add constant velocity in the first column
                    if (i == 1)
                    {
                        f.u[i * n + j] = inVel;
                    }
                }
            }

            //Add smoke
            float pipeH = 0.1f * f.numY;
            
            int minJ = Mathf.FloorToInt(0.5f * f.numY - 0.5f * pipeH);
            int maxJ = Mathf.FloorToInt(0.5f * f.numY + 0.5f * pipeH);

            for (var j = minJ; j < maxJ; j++)
            {
                f.m[j] = 0f; //Why is this 0???
            }


            //setObstacle(0.4, 0.5, true);


            scene.gravity = 0f; //???
            scene.showPressure = false;
            scene.showSmoke = true;
            scene.showStreamlines = false;
            scene.showVelocities = false;

            if (sceneNr == Scene.SceneNr.HighResWindTunnel)
            {
                //scene.dt = 1.0 / 120.0;
                scene.numIters = 100;
                scene.showPressure = true;
            }
        }
        else if (sceneNr == Scene.SceneNr.Paint)
        {
            scene.gravity = 0f;
            scene.overRelaxation = 1f;
            scene.showPressure = false;
            scene.showSmoke = true;
            scene.showStreamlines = false;
            scene.showVelocities = false;
            scene.obstacleRadius = 0.1f;
        }
    }



    //UI
    private void OnGUI()
    {
        fluidUI.DisplayUI(scene);
    }
}