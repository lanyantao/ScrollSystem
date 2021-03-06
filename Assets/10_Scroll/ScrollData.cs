﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public class ScrollData
	{

		public ScrollData() { }

		public ScrollData(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> onResize)
		{
			Init(scrollSystem, prefabName, dataSource, onResize);
		}

		protected void Init(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, Vector2> onResize)
		{
			this.scrollSystem = scrollSystem;
			this.objectPool = scrollSystem.ObjectPoolDic[prefabName];
			this.dataSource = dataSource;
			this.newLine = objectPool.newLine;
			this.onResize = onResize;
			this.isPositionInited = false;
		}

		//基本的属性值
		public float width;
		public float height;
		public ScrollLayout.NewLine newLine;
		public ScrollSystem.ObjectPool objectPool;
		public System.Object dataSource;
		public ScrollSystem scrollSystem;
		public Vector2 anchoredPosition;

		private Func<object, Vector2> onResize;
		public bool isVisible { get; private set; }
		public bool isPositionInited { get; private set; }

		private RectBounds rectBounds = new RectBounds();
		private uint lastUpdateFrame = 0;
		private RectTransform targetTrans = null;

		public Vector2 Size
		{
			get
			{
				return new Vector2(width, height);
			}
		}

		public Vector3 GetWorldPosition()
		{
			return Tools.GetWorldPosByAnchoredPos(scrollSystem.contentTrans, anchoredPosition, scrollSystem.PrefabAnchor);
		}

		/// <summary>
		/// 设置宽度和高度
		/// </summary>
		public bool OnResize()
		{
			bool changed = false;
			if (onResize != null)
			{
				var newSize = onResize(dataSource);
				if (newSize.x > 0)
				{
					if (this.width != newSize.x)
					{
						this.width = newSize.x;
						changed = true;
					}
				}
				else
				{
					this.width = objectPool.prefabWidth;
				}
				if (newSize.y > 0)
				{
					if (this.height != newSize.y)
					{
						this.height = newSize.y;
						changed = true;
					}
				}
				else
				{
					this.height = objectPool.prefabHeight;
				}
			}
			else
			{
				var rectTrans = objectPool.origin.transform as RectTransform;
				this.width = rectTrans.sizeDelta.x;
				this.height = rectTrans.sizeDelta.y;
			}
			return changed;
		}

		public void Hide()
		{
			this.isVisible = false;
			if (this.targetTrans != null)
			{
				objectPool.Recycle(this.targetTrans.gameObject);
				this.targetTrans = null;
			}
		}

		/// <summary>
		/// 更新内容
		/// </summary>
		/// <param name="refresh">表示强制刷新</param>
		public void Update(bool refreshContent, bool refreshPosition)
		{
			if (isVisible)
			{
				if (this.targetTrans == null)
				{
					this.targetTrans = objectPool.Get().transform as RectTransform;
					refreshContent = true;
					refreshPosition = true;
				}
				if (refreshPosition)
				{
					//Debug.Log("refreshPosition");
					this.targetTrans.sizeDelta = new Vector2(this.width, this.height);
					this.targetTrans.anchoredPosition = anchoredPosition;
#if UNITY_EDITOR
					ShowGizmosBounds();
#endif
				}
				if (refreshContent)
				{
					//Debug.Log("refreshContent");
					if (this.scrollSystem.setItemContent != null) {
						this.scrollSystem.setItemContent(objectPool.prefabName, this.targetTrans, dataSource);
					}
				}
			}
		}

		public void ShowGizmosBounds()
		{
			if (scrollSystem.DrawGizmos)
			{
				Tools.DrawRectBounds(GetWorldPosition(), scrollSystem.contentTrans.lossyScale.x * width, scrollSystem.contentTrans.lossyScale.y * height, Color.red);
			}
		}

		/// <summary>
		/// 只检查是否可见
		/// </summary>
		public void CheckVisible(uint frame)
		{
			if (frame > lastUpdateFrame)
			{
				lastUpdateFrame = frame;
				isVisible = rectBounds.Overlaps(scrollSystem.scrollBounds);
			}
		}

		/// <summary>
		/// 设置位置
		/// </summary>
		public void SetAnchoredPosition(Vector2 originPosition)
		{
			this.isPositionInited = true;
			if (scrollSystem.Centered && (newLine == ScrollLayout.NewLine.None))
			{
				 
				//只是临时存储用
				this.anchoredPosition = originPosition;
			}
			else
			{
				this.anchoredPosition = scrollSystem.TransAnchoredPosition(originPosition);
				UpdateRectBounds();
			}
		}

		/// <summary>
		/// 设置居中偏移量
		/// </summary>
		public void SetCenterOffset(Vector2 offset)
		{
			if (newLine == ScrollLayout.NewLine.None)
			{
				this.anchoredPosition = scrollSystem.TransAnchoredPosition(this.anchoredPosition + offset);
				UpdateRectBounds();
			}
		}

		private void UpdateRectBounds()
		{
			this.rectBounds.left = anchoredPosition.x - 0.5f * width;
			this.rectBounds.right = anchoredPosition.x + 0.5f * width;
			this.rectBounds.up = anchoredPosition.y + 0.5f * height;
			this.rectBounds.down = anchoredPosition.y - 0.5f * height;
		}

	}
}