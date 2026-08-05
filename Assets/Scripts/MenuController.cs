using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    [Header("Select")]

    [SerializeField]
    private TMP_InputField input;

    [Header("Random")]
    [SerializeField]
    private TMP_InputField amountOfValues;
    [SerializeField]
    private TMP_InputField minRange;
    [SerializeField]
    private TMP_InputField maxRange;

    public static float[] values;
    public void StartSim()
    {
        values = input.text
        .Split(' ')                              
        .Where(s => float.TryParse(s, out _))
        .Select(float.Parse)                    
        .ToArray();

        if (values.Length <= 0)
            return;

        SceneManager.LoadScene("Simulation");
    }

    public void RandomStart()
    {
        //if (amountOfValues.text == "" || minRange.text == "" || maxRange.text == "")
            //return;

        if (int.TryParse(amountOfValues.text, out int amount) && float.TryParse(minRange.text, out float min) && float.TryParse(maxRange.text, out float max))
        {
            values = new float[amount];

            for (int i = 0; i < amount; i++)
            {
                values[i] = Random.Range(min, max);
            }

            SceneManager.LoadScene("Simulation");
        }
    }
}
