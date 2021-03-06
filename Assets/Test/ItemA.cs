﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//表示自增
public class ItemA : MonoBehaviour
{

	private Text label;
	private Button button;
	private SimpleData data;

	private void Awake()
	{
		label = this.transform.Find("Text").GetComponent<Text>();
		button = this.GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	public void UpdateInfo(SimpleData data)
	{
		this.data = data;
		label.text = data.index.ToString();
	}

	public void OnClick()
	{
		data.index++;
		UpdateInfo(data);
	}

}
