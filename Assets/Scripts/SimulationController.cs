using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    [SerializeField]
    private GameObject valueGraph;

    [SerializeField]
    private GameObject yMark;

    [SerializeField]
    private GameObject[] valueGraphs;

    [SerializeField]
    private float[] _values;

    [SerializeField]
    private Slider simSpeedSlider;

    private float simSpeed;

    private bool sorting;

    [SerializeField]
    private TMP_Dropdown sortIndex;

    [SerializeField]
    private Toggle manual;

    [SerializeField]
    private Sprite[] playAndStopIcon;

    [SerializeField]
    private TMP_Text barText;

    [SerializeField]
    private Image button;
    private void Start()
    {
        _values = MenuController.values.ToArray();

        float biggestGraph = 0;
        float smallestGraph = 0;

        foreach (float value in _values)
        {
            if (Mathf.Abs(value) > biggestGraph)
                biggestGraph = value;

            if (value < smallestGraph)
                smallestGraph = value;
        }

        Camera.main.orthographicSize = biggestGraph / 2;

        valueGraphs = new GameObject[_values.Length];

        float totalWidth = _values.Length * 0.3f;
        float xMultiplier = AdjustCameraSize(totalWidth, biggestGraph, smallestGraph);

        totalWidth *= xMultiplier;

        float yPosition = smallestGraph < 0 ? 0 : Camera.main.orthographicSize * -1;

        int numMarks = 10;
        SpawnLines(yPosition, totalWidth, numMarks);

        for (int i = 0; i < _values.Length; i++)
        {
            float xPosition = i * 0.3f * xMultiplier - totalWidth / 2;
            float barHeight = _values[i] / 4;
            float yPositionForBar = yPosition + barHeight;

            valueGraphs[i] = Instantiate(valueGraph, new Vector2(xPosition, yPositionForBar), Quaternion.identity);
            valueGraphs[i].transform.localScale = new Vector3(valueGraphs[i].transform.localScale.x * xMultiplier, _values[i] / 2, 1);
        }
    }


    private float AdjustCameraSize(float totalWidth, float biggestGraph, float smallestGraph)
    {
        Camera mainCamera = Camera.main;
        float aspectRatio = mainCamera.aspect;

        float horizontalSizeNeeded = totalWidth / (2 * aspectRatio) + 1;

        float verticalSizeNeeded = Mathf.Max(biggestGraph, Mathf.Abs(smallestGraph)) + 1;

        mainCamera.orthographicSize = Mathf.Max(horizontalSizeNeeded, verticalSizeNeeded);

        if (verticalSizeNeeded > horizontalSizeNeeded)
            return verticalSizeNeeded / horizontalSizeNeeded;
        else
            return 1;
    }

    private void SpawnLines(float yPosition, float totalWidth, int numMarks)
    {
        float yMarkThickness = Camera.main.orthographicSize / 400;
        float markSpacing = Camera.main.orthographicSize * 2 / (numMarks - 1);

        for (int i = -numMarks + 1; i <= numMarks - 1; i++)
        {
            float yMarkPosition = yPosition + i * markSpacing;

            Instantiate(yMark, new Vector3(0, yMarkPosition), Quaternion.identity)
                .transform.localScale = new Vector3(totalWidth * 5, yMarkThickness, 1);

            float orthographicWidth = Camera.main.orthographicSize * Camera.main.aspect;

            var text = Instantiate(barText, new Vector3((orthographicWidth * -1) + orthographicWidth / 30, yMarkPosition + Camera.main.orthographicSize / 20), Quaternion.identity);

            text.text = (Mathf.Ceil((yMarkPosition - yPosition) * 20) / 10).ToString();
            text.rectTransform.sizeDelta = new Vector2(1000, 20);
            text.fontSize = Camera.main.orthographicSize / 2;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Restart();

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu");

        if (Input.GetKeyDown(KeyCode.Space))
            StartSorting();

        simSpeed = 1 - simSpeedSlider.value;
    }

    public void Restart()
    {
        StopAllCoroutines();
        button.sprite = playAndStopIcon[1];

        foreach (var valueGraph in valueGraphs)
            Destroy(valueGraph);

        _values = MenuController.values.ToArray();

        float biggestGraph = 0;
        float smallestGraph = 0;

        foreach (float value in _values)
        {
            if (Mathf.Abs(value) > biggestGraph)
                biggestGraph = value;

            if (value < smallestGraph)
                smallestGraph = value;
        }

        valueGraphs = new GameObject[_values.Length];

        float totalWidth = _values.Length * 0.3f;
        float xMultiplier = AdjustCameraSize(totalWidth, biggestGraph, smallestGraph);

        totalWidth *= xMultiplier;

        float yPosition = smallestGraph < 0 ? 0 : Camera.main.orthographicSize * -1;

        for (int i = 0; i < _values.Length; i++)
        {
            float xPosition = i * 0.3f * xMultiplier - totalWidth / 2;
            float barHeight = _values[i] / 4;
            float yPositionForBar = yPosition + barHeight;

            valueGraphs[i] = Instantiate(valueGraph, new Vector2(xPosition, yPositionForBar), Quaternion.identity);
            valueGraphs[i].transform.localScale = new Vector3(valueGraphs[i].transform.localScale.x * xMultiplier, _values[i] / 2, 1);
        }
    }

    public void StartSorting()
    {
        if (sorting)
        {
            sorting = false;
            StopAllCoroutines();
            foreach (GameObject graph in valueGraphs)
            {
                graph.GetComponent<SpriteRenderer>().color = Color.white;
            }

            button.sprite = playAndStopIcon[1];
        }
        else
        {
            button.sprite = playAndStopIcon[0];
            sorting = true;

            switch (sortIndex.value)
            {
                case 0:
                    StartCoroutine(SelectionSort());
                    break;

                case 1:
                    StartCoroutine(BubbleSort());
                    break;

                case 2:
                    StartCoroutine(InsertionSort());
                    break;

                case 3:
                    StartCoroutine(MergeSort());
                    break;

                case 4:
                    StartCoroutine(QuickSort());
                    break;
            }
        }

    }
    void Switch(int a, int b)
    {
        float sizeA = _values[a];
        float sizeB = _values[b];

        _values[a] = sizeB;
        _values[b] = sizeA;

        float posA = valueGraphs[a].transform.position.x;
        float posB = valueGraphs[b].transform.position.x;

        valueGraphs[a].transform.position = new Vector3(posB, valueGraphs[a].transform.position.y, 1);
        valueGraphs[b].transform.position = new Vector3(posA, valueGraphs[b].transform.position.y, 1);

        GameObject graphA = valueGraphs[a];
        GameObject graphB = valueGraphs[b];

        valueGraphs[a] = graphB;
        valueGraphs[b] = graphA;
    }
    private void ResetBarColors()
    {
        foreach (var graph in valueGraphs)
            graph.GetComponent<SpriteRenderer>().color = Color.white;
    }

    IEnumerator Pause()
    {
        if (manual.isOn)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            yield return new WaitUntil(() => !Input.GetKey(KeyCode.Space));
        }
        else
        {
            yield return new WaitForSeconds(simSpeed);
        }
    }
    private IEnumerator Check()
    {
        for (int i = 0; i < _values.Length - 1; i++)
        {

            if (_values[i] > _values[i + 1])
            {
                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.red;
                break;
            }
            else
                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.green;

            yield return Pause();
        }

        valueGraphs[valueGraphs.Length - 1].GetComponent<SpriteRenderer>().color = Color.green;
    }
    private IEnumerator SelectionSort()
    {
        float minSize;
        int minSizeValue;

        for (int j = 0; j < _values.Length; j++)
        {
            minSize = Mathf.Infinity;
            minSizeValue = j;

            for (int i = j; i < _values.Length; i++)
            {
                if (_values[i] < minSize)
                {
                    minSize = _values[i];
                    minSizeValue = i;

                    valueGraphs[i].GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0, 1);

                }
                else
                {
                    valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.yellow;
                }

                yield return Pause();

                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.white;
            }

            Switch(j, minSizeValue);

            valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.cyan;
            valueGraphs[minSizeValue].GetComponent<SpriteRenderer>().color = Color.cyan;

            yield return Pause(); 

            valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.white;
            valueGraphs[minSizeValue].GetComponent<SpriteRenderer>().color = Color.white;
        }

        StartCoroutine(Check());
    }

    IEnumerator BubbleSort()
    {
        for (int j = 1; j < _values.Length; j++)
        {
            bool switched = false;

            for (int i = 0; i < _values.Length - j; i++)
            {
                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.cyan;
                valueGraphs[i + 1].GetComponent<SpriteRenderer>().color = Color.cyan;

                if (_values[i] > _values[i + 1])
                {
                    Switch(i, i + 1);
                    switched = true;
                }
                else
                {
                    valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.yellow;
                    valueGraphs[i + 1].GetComponent<SpriteRenderer>().color = Color.yellow;
                }

                yield return Pause();

                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.white;
                valueGraphs[i + 1].GetComponent<SpriteRenderer>().color = Color.white;
            }

            if (!switched)
                break;
        }

        StartCoroutine(Check());
    }


    private IEnumerator InsertionSort()
    {
        for (int i = 1; i < _values.Length; i++)
        {
            for (int j = i; j > 0 && _values[j - 1] > _values[j]; j--)
            {
                Switch(j - 1, j);

                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.cyan;
                valueGraphs[j - 1].GetComponent<SpriteRenderer>().color = Color.cyan;

                yield return Pause();

                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.white;
                valueGraphs[j - 1].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        StartCoroutine(Check());
    }


    private IEnumerator MergeSort()
    {
        yield return StartCoroutine(DivideMS(0, _values.Length - 1));
        StartCoroutine(Check()); 
    }

    private IEnumerator DivideMS(int left, int right)
    {
        if (left < right)
        {
            int middle = (left + right) / 2;

            yield return StartCoroutine(DivideMS(left, middle));
            yield return StartCoroutine(DivideMS(middle + 1, right));
            yield return StartCoroutine(Merge(left, middle, right));
        }
    }

    private IEnumerator Merge(int left, int middle, int right)
    {
        int i = left;        
        int j = middle + 1;              

        while (i <= middle && j <= right)
        {
            valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.cyan;
            valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.yellow;

            if (_values[i] > _values[j])
            {
                for (int s = j; s > i; s--)
                {
                    Switch(s, s - 1);
                    yield return Pause();
                }

                middle++;
                j++;
            }

            StartCoroutine(Pause());

            ResetBarColors();

            i++;
        }
        ResetBarColors();
    }

    IEnumerator QuickSort()
    {
        yield return StartCoroutine(DivideQS(0, _values.Length - 1));
        StartCoroutine(Check());
    }

    IEnumerator DivideQS(int min, int max)
    {
        if (min >= max || min < 0 || max >= _values.Length)
            yield break;

        float pivotValue = _values[min];
        int i = min + 1;  
        int j = max;      

        bool done = false;

        while (!done)
        {
            while (i <= max && _values[i] < pivotValue)
            {
                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.yellow;
                yield return Pause();
                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.white;
                i++;
            }

            while (j >= min && _values[j] > pivotValue)
            {
                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.yellow;
                yield return Pause();
                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.white;
                j--;
            }

            if (i <= j)
            {
                Switch(i, j); 

                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.cyan;
                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.cyan;

                yield return Pause();

                valueGraphs[i].GetComponent<SpriteRenderer>().color = Color.white;
                valueGraphs[j].GetComponent<SpriteRenderer>().color = Color.white;
                i++;
                j--;
            }
            else
            {
                done = true; 
            }
        }

        Switch(min, j);

        yield return Pause();

        if (min < j)
            yield return StartCoroutine(DivideQS(min, j));
        if (i < max)
            yield return StartCoroutine(DivideQS(i, max));
    }
}
