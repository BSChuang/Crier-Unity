using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google;

public class OptionPage : MonoBehaviour {
    public Crier crier;
    public FB fb;
    public MapApi mapApi;
    public Profile profile;
    public Toggle includeRestaurants;
    public Dropdown sortDropdown;
    public InputField keywordField;
    public GameObject debugPage;
    public Dropdown sortByDropdown;


    public void ActivePage() {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void OnKeywordChange() {
        crier.keyword = keywordField.text;
    }

    public void IncludeRestaurants() {
        if (includeRestaurants.isOn) {
            mapApi.categories = "categories=restaurants";
        } else {
            mapApi.categories = "categories=bars";
        }
    }

    public void DebugButton() {
        debugPage.SetActive(!debugPage.activeSelf);
    }
}
