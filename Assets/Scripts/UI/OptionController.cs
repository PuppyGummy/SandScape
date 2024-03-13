using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    public List<UI_ImageFlipFlop> options = new List<UI_ImageFlipFlop>();
    private int currentOption = 1;

    public void ChangeOption(UI_ImageFlipFlop newSelection)
    {
        options[currentOption].Flip();
        newSelection.Flip();

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] == newSelection)
            {
                currentOption = i;
            }
        }
    }
}
