using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SkillSystem : MonoBehaviour
{
    [SerializeField] private UISkill[] _skills;

    private void Update()
    {
        if (Input.anyKeyDown && int.TryParse(Input.inputString, out int key) && key >= 1 && key <= _skills.Length)
        {
            _skills[key - 1].UseSkill();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _skills[0].UseSkill();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _skills[1].UseSkill();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _skills[2].UseSkill();
        }
    }

}
