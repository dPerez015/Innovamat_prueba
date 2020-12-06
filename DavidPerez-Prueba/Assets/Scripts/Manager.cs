using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Manager : MonoBehaviour
{
    public enum Idioma { Catala,Castellano};

    [Header("Seleccion de Idioma\n")]
    public Idioma idioma;
    //Holds references to each ScriptableObject with the names of numbers in each languaje
    public NumberNames[] namesLists =new NumberNames[Enum.GetValues(typeof(Idioma)).Length];

    [Header("Main Text")]
    public Text nameText;
    public float introTime = 2;
    public float holdTime = 2;
    public float hideTime = 2;

    [Header("Buttons")]
    public Button[] buttons;
    Text[] buttonTexts;
    public float buttonsIntroTime = 2;
    public float buttonsOutTime = 1;

    //Exercise info
    int correctNumber;
    int correctButtonIdx;
    bool oneFail;
    bool exerciseEnded;

    [Header("Counters")]
    public Text failedAttemptsCounter;
    int failedAttempts;
    public Text correctAttemptsCounter;
    int correctAttempts;
    

    void GetButtonsTexts()
    {
        buttonTexts = new Text[buttons.Length];
        for(int i = 0; i < buttons.Length; i++)
        {
            buttonTexts[i] = buttons[i].transform.GetChild(0).GetComponent<Text>();
        }
    }

    void Start()
    {
        //get reference to each button Text component
        GetButtonsTexts();

        //initialize the correct number to -1 since we check that it is diferent from last try, 
        correctNumber = -1;

        //set attempts to 0
        failedAttempts = 0;
        failedAttempts = 0;
        failedAttemptsCounter.text = "0";
        correctAttemptsCounter.text = "0";

        //Start the exercise at first update
        exerciseEnded = true;
    }

    // Update is called once per frame
    void Update()
    {  
        if (exerciseEnded)
        {
            StartTry();
        }
    }

    private void ResetExercice()
    {
        //reset Text color
        nameText.color = new Color(0, 0, 0, 0);

        //make sure buttons are not interactable & reset color values
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
            buttons[i].image.color = new Color(1, 1, 1, 0);
            buttonTexts[i].color = new Color(0, 0, 0, 0);       
        }

        //reset bool for one option to fail & make exercise not ended
        oneFail = false;
        exerciseEnded = false;
    }

    void StartTry()
    {
        ResetExercice();

        //Select the correct number for this try that is diferent from last one
        int newCorrectNumber = UnityEngine.Random.Range(0, namesLists[(int)idioma].names.Length);
        while(newCorrectNumber==correctNumber)
            newCorrectNumber = UnityEngine.Random.Range(0, namesLists[(int)idioma].names.Length);
        correctNumber = newCorrectNumber;
        nameText.text = namesLists[(int)idioma].names[correctNumber];

        //Select a random button to take the correct value
        correctButtonIdx = UnityEngine.Random.Range(0, buttonTexts.Length);
        buttonTexts[correctButtonIdx].text = correctNumber.ToString();

        //Select the incorrect numbers making them diferent from one another and from correctNumber
        int[] incorrectNum = new int[buttonTexts.Length];
        incorrectNum[correctButtonIdx] = correctNumber;

        for(int i = 0; i < buttonTexts.Length; i++)
        {
            //if it's not the correct one
            if (i != correctButtonIdx)
            {
                //We generate a new random number
                int newNum = UnityEngine.Random.Range(0, namesLists[(int)idioma].names.Length);

                //we check if it's different from the correct number
                bool isDifferent = newNum != correctNumber;

                //if its valid we test if it's different from the already generated incorrectNumbers
                for (int j = 0; j < i && isDifferent; j++)
                {
                    if (newNum == incorrectNum[j])
                        isDifferent = false;
                }
                //if it is still valid we keep the value other wise we take i a step back to repeat the process
                if (isDifferent && i < buttonTexts.Length)
                {
                    incorrectNum[i] = newNum;
                    buttonTexts[i].text = newNum.ToString();
                }
                else
                    i--;
            }
        }
        
        //Start the animation corrutine
        StartCoroutine(StartAnimation());

    }

    IEnumerator StartAnimation()
    {
        //Fade in
        float currentTime = 0;
        while (currentTime<introTime)
        {
            nameText.color = new Color(0, 0, 0, currentTime / introTime);
            yield return null;
            currentTime += Time.deltaTime;
        }

        //Hold time
        //If we want more precision we might want to wait for holdTime-(introTime-currentTime)
        yield return new WaitForSeconds(holdTime);

        //Fade out, we make the counter go backwards to avoid having to calculate the HideTime-currentTime everyframe (not really that important)
        //Again, if we wanted precision we needed to take into account the small amount of time lost between holdTime and actual time
        currentTime = hideTime;
        while(currentTime > 0)
        {
            nameText.color = new Color(0, 0, 0, currentTime / hideTime);
            yield return null;
            currentTime -= Time.deltaTime;   
        }

        //Buttons fade in animation
        currentTime = 0; 

        while(currentTime < buttonsIntroTime)
        {
            for(int i =0; i < buttonTexts.Length; i++)
            {
                buttonTexts[i].color = new Color(0, 0, 0, currentTime / hideTime);
                buttons[i].image.color = new Color(1, 1, 1, currentTime / hideTime);
            }
            yield return null;
            currentTime += Time.deltaTime;
        }

        //Make buttons interactable
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
    }


    public void ButtonsAnimation(int id)
    {
        if (id == correctButtonIdx)
        {
            buttons[id].image.color = Color.green;
            for(int i = 0; i < buttons.Length; i++)
            {
                //if it hasn't faded yet we make it fade
                if(buttons[i].IsInteractable())
                    StartCoroutine(ButtonsFadeOutAnimation(i));
            }
            correctAttempts++;
            correctAttemptsCounter.text = correctAttempts.ToString();

        }
        else
        {
            buttons[id].image.color = Color.red;
            StartCoroutine(ButtonsFadeOutAnimation(id));
            //If already failed once, exercise solved, all fade
            if (oneFail)
            {
                buttons[correctButtonIdx].image.color = Color.green;
                StartCoroutine(ButtonsFadeOutAnimation(correctButtonIdx));
                failedAttempts++;
                failedAttemptsCounter.text = failedAttempts.ToString();
            }
            else
                oneFail = true;
        }
    }

    IEnumerator ButtonsFadeOutAnimation(int id) {
        float currentTime = 0;
        buttons[id].interactable = false;
        while (currentTime < buttonsOutTime)
        {
            buttons[id].image.color -= new Color(0, 0, 0, Time.deltaTime / buttonsOutTime);
            buttonTexts[id].color = new Color(1, 1, 1, 1-(currentTime / buttonsOutTime));
            yield return null;
            currentTime += Time.deltaTime;
        }

        if (id == correctButtonIdx)
            exerciseEnded = true;
    }

}
