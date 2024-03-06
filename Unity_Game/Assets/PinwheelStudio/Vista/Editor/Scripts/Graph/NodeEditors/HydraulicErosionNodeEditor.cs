#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(HydraulicErosionNode))]
    public class HydraulicErosionNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent GENERAL_HEADER = new GUIContent("General");
        private static readonly GUIContent HIGH_QUALITY = new GUIContent("High Quality", "If true, it will simulate water flow in 8 directions, otherwise 4");
        private static readonly GUIContent DETAIL_LEVEL = new GUIContent("Detail Level", "Smaller value runs faster and produces larger features, while larger value is more expensive but produces more micro details");
        private static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration", "The number of simulation step to perform");
        private static readonly GUIContent ITERATION_PER_FRAME = new GUIContent("Iteration Per Frame", "The number of step to perform in a single frame");

        private static readonly GUIContent SIMULATION_HEADER = new GUIContent("Simulation");
        private static readonly GUIContent WATER_SOURCE_AMOUNT = new GUIContent("Water Source", "The amount of water pour into the system in each iteration");
        private static readonly GUIContent WATER_SOURCE_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the water source amount without modifying its base value");

        private static readonly GUIContent FLOW_RATE = new GUIContent("Flow Rate", "Water flow speed. Default value is fine, too high may cause numerical error");
        private static readonly GUIContent FLOW_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the flow speed without modifying its base value");

        private static readonly GUIContent SEDIMENT_CAPACITY = new GUIContent("Sediment Capacity", "The amount of sediment that water can carry. Default value is fine.");
        private static readonly GUIContent SEDIMENT_CAPACITY_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the sediment capacity without modifying its base value");

        private static readonly GUIContent EROSION_RATE = new GUIContent("Erosion Rate", "Strength of the erosion, higher value will pick up more soil and carve deeper into the terrain");
        private static readonly GUIContent EROSION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the erosion strength without modifying its base value");

        private static readonly GUIContent DEPOSITION_RATE = new GUIContent("Deposition Rate", "Strength of the deposition, higher value will add more soil back to the terrain, while lower value will make the deposition wide spread");
        private static readonly GUIContent DEPOSITION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the deposition strength without modifying its base value");
        private static readonly GUIContent DEPOSITION_SLOPE_FACTOR = new GUIContent("Slope Factor", "Decide the slope steepness where soil can deposit");

        private static readonly GUIContent EVAPORATION_RATE = new GUIContent("Evaporation Rate", "Strength of the evaporation that remove water from the system");
        private static readonly GUIContent EVAPORATION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the evaporation strength without modifying its base value");

        private static readonly GUIContent BED_ROCK_DEPTH = new GUIContent("Bed Rock Depth", "Depth of the bed rock layer where it's barely eroded.");
        private static readonly GUIContent SHARPNESS = new GUIContent("Sharpness", "Control the sharpness of eroded features.");

        private static readonly GUIContent ARTISTIC_HEADER = new GUIContent("Artistic Controls");
        private static readonly GUIContent HEIGHT_SCALE = new GUIContent("Height Scale", "A multiplier to terrain height to further enhance the erosion effect");
        private static readonly GUIContent DETAIL_HEIGHT_SCALE = new GUIContent("Detail Height Scale", "A multiplier to the detail height map to randomize the water flow and create more eroded features on the very flat areas");
        private static readonly GUIContent EROSION_BOOST = new GUIContent("Erosion Boost", "A multiplier to enhance the erosion effect");
        private static readonly GUIContent DEPOSITION_BOOST = new GUIContent("Deposition Boost", "A multiplier to enhance the deposition effect");

        private static readonly GUIContent UTIL_HEADER = new GUIContent("Utilities");
        private static readonly GUIContent SELECT_TEMPLATE = new GUIContent("Select Template...");

        public override void OnGUI(INode node)
        {
            HydraulicErosionNode n = node as HydraulicErosionNode;
            EditorGUI.BeginChangeCheck();

            EditorCommon.Header(GENERAL_HEADER);
            bool highQuality = EditorGUILayout.Toggle(HIGH_QUALITY, n.highQualityMode);
            float detailLevel = EditorGUILayout.Slider(DETAIL_LEVEL, n.detailLevel, 0f, 1f);
            int iterationCount = EditorGUILayout.IntField(ITERATION_COUNT, n.iterationCount);
            int iterationPerFrame = EditorGUILayout.IntField(ITERATION_PER_FRAME, n.iterationPerFrame);

            EditorCommon.Header(SIMULATION_HEADER);
            float waterSourceAmount = EditorGUILayout.FloatField(WATER_SOURCE_AMOUNT, n.waterSourceAmount);
            float waterSourceMultiplier = EditorGUILayout.Slider(WATER_SOURCE_MULTIPLIER, n.waterSourceMultiplier, 0f, 2f);

            float flowRate = EditorGUILayout.FloatField(FLOW_RATE, n.flowRate);
            float flowMultiplier = EditorGUILayout.Slider(FLOW_MULTIPLIER, n.flowMultiplier, 0f, 2f);

            float sedimentCapacity = EditorGUILayout.FloatField(SEDIMENT_CAPACITY, n.sedimentCapacity);
            float sedimentMultiplier = EditorGUILayout.Slider(SEDIMENT_CAPACITY_MULTIPLIER, n.sedimentCapacityMultiplier, 0f, 2f);

            float erosionRate = EditorGUILayout.FloatField(EROSION_RATE, n.erosionRate);
            float erosionMultiplier = EditorGUILayout.Slider(EROSION_MULTIPLIER, n.erosionMultiplier, 0f, 2f);

            float depositionRate = EditorGUILayout.FloatField(DEPOSITION_RATE, n.depositionRate);
            float depositionMultiplier = EditorGUILayout.Slider(DEPOSITION_MULTIPLIER, n.depositionMultiplier, 0f, 2f);

            float evaporationRate = EditorGUILayout.FloatField(EVAPORATION_RATE, n.evaporationRate);
            float evaporationMultiplier = EditorGUILayout.Slider(EVAPORATION_MULTIPLIER, n.evaporationMultiplier, 0f, 2f);

            float bedRockDepth = EditorGUILayout.FloatField(BED_ROCK_DEPTH, n.bedRockDepth);
            float sharpness = EditorGUILayout.Slider(SHARPNESS, n.sharpness, 0f, 1f);

            EditorCommon.Header(ARTISTIC_HEADER);
            float heightScale = EditorGUILayout.FloatField(HEIGHT_SCALE, n.heightScale);
            float detailHeightScale = EditorGUILayout.FloatField(DETAIL_HEIGHT_SCALE, n.detailHeightScale);
            float erosionBoost = EditorGUILayout.FloatField(EROSION_BOOST, n.erosionBoost);
            float depositionBoost = EditorGUILayout.FloatField(DEPOSITION_BOOST, n.depositionBoost);
            float depositionSlopeFactor = EditorGUILayout.FloatField(DEPOSITION_SLOPE_FACTOR, n.depositionSlopeFactor);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.highQualityMode = highQuality;
                n.detailLevel = detailLevel;
                n.iterationCount = iterationCount;
                n.iterationPerFrame = iterationPerFrame;

                n.waterSourceAmount = waterSourceAmount;
                n.waterSourceMultiplier = waterSourceMultiplier;

                n.flowRate = flowRate;
                n.flowMultiplier = flowMultiplier;

                n.sedimentCapacity = sedimentCapacity;
                n.sedimentCapacityMultiplier = sedimentMultiplier;

                n.erosionRate = erosionRate;
                n.erosionMultiplier = erosionMultiplier;

                n.depositionRate = depositionRate;
                n.depositionMultiplier = depositionMultiplier;

                n.evaporationRate = evaporationRate;
                n.evaporationMultiplier = evaporationMultiplier;

                n.bedRockDepth = bedRockDepth;
                n.sharpness = sharpness;

                n.heightScale = heightScale;
                n.detailHeightScale = detailHeightScale;
                n.erosionBoost = erosionBoost;
                n.depositionBoost = depositionBoost;
                n.depositionSlopeFactor = depositionSlopeFactor;
            }

            EditorCommon.Header(UTIL_HEADER);
            Rect templateRect = EditorGUILayout.GetControlRect();
            if (GUI.Button(templateRect, SELECT_TEMPLATE))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Large features (faster, larger, smoother flow, creating the overall shape)"),
                    false,
                    () =>
                    {
                        m_graphEditor.RegisterUndo(n);
                        n.highQualityMode = true; n.detailLevel = 0.2f;
                        n.iterationCount = 10; n.iterationPerFrame = 1;
                        n.waterSourceAmount = 0.5f; n.waterSourceMultiplier = 1f;
                        n.flowRate = 1f; n.flowMultiplier = 1f;
                        n.sedimentCapacity = 5f; n.sedimentCapacityMultiplier = 1f;
                        n.erosionRate = 1f; n.erosionMultiplier = 1f;
                        n.depositionRate = 1f; n.depositionMultiplier = 1f;
                        n.evaporationRate = 0.1f; n.evaporationMultiplier = 1f;
                        n.bedRockDepth = 20f;
                        n.sharpness = 0f;
                        n.heightScale = 1f;
                        n.detailHeightScale = 1f;
                        n.erosionBoost = 1f;
                        n.depositionBoost = 1f;
                        n.depositionSlopeFactor = 1f;
                        m_graphEditor.ExecuteGraph();
                    });

                menu.AddItem(
                    new GUIContent("Medium details (quality-performance balanced, eye-pleasant flows)"),
                    false,
                    () =>
                    {
                        m_graphEditor.RegisterUndo(n);
                        n.highQualityMode = true; n.detailLevel = 0.4f;
                        n.iterationCount = 20; n.iterationPerFrame = 1;
                        n.waterSourceAmount = 0.15f; n.waterSourceMultiplier = 1f;
                        n.flowRate = 1f; n.flowMultiplier = 1f;
                        n.sedimentCapacity = 2.5f; n.sedimentCapacityMultiplier = 1f;
                        n.erosionRate = 1f; n.erosionMultiplier = 1f;
                        n.depositionRate = 1f; n.depositionMultiplier = 1f;
                        n.evaporationRate = 0.1f; n.evaporationMultiplier = 1f;
                        n.bedRockDepth = 15f;
                        n.sharpness = 0.5f;
                        n.heightScale = 1f;
                        n.detailHeightScale = 1f;
                        n.erosionBoost = 1f;
                        n.depositionBoost = 1f;
                        n.depositionSlopeFactor = 1f;
                        m_graphEditor.ExecuteGraph();
                    });

                menu.AddItem(
                    new GUIContent("Micro details (slower, smaller, sharper flow, more detail for texturing)"),
                    false,
                    () =>
                    {
                        m_graphEditor.RegisterUndo(n);
                        n.highQualityMode = true; n.detailLevel = 1f;
                        n.iterationCount = 30; n.iterationPerFrame = 1;
                        n.waterSourceAmount = 0.15f; n.waterSourceMultiplier = 1f;
                        n.flowRate = 1f; n.flowMultiplier = 1f;
                        n.sedimentCapacity = 2.5f; n.sedimentCapacityMultiplier = 1f;
                        n.erosionRate = 0.5f; n.erosionMultiplier = 1f;
                        n.depositionRate = 1f; n.depositionMultiplier = 1f;
                        n.evaporationRate = 0.12f; n.evaporationMultiplier = 1f;
                        n.bedRockDepth = 10f;
                        n.sharpness = 1f;
                        n.heightScale = 1f;
                        n.detailHeightScale = 1f;
                        n.erosionBoost = 1f;
                        n.depositionBoost = 1f;
                        n.depositionSlopeFactor = 1f;
                        m_graphEditor.ExecuteGraph();
                    });
                menu.DropDown(templateRect);
            }
        }
    }
}
#endif
