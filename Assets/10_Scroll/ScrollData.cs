﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	public class ScrollData
	{

		public static Vector2 DEFAULT_ANCHOR = new Vector2(0, 1);

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

		public Vector3 worldPosition { get; private set; }
		private Func<object, Vector2> onResize;
		public bool isVisible { get; private set; }
		public bool isPositionInited { get; private set; }

		private RectBounds rectBounds = new RectBounds();
		private uint lastUpdateFrame = 0;
		private GameObject targetGo = null;

		public Vector2 Size
		{
			get
			{
				return new Vector2(width, height);
			}
		}

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
			if (this.targetGo != null)
			{
				objectPool.Recycle(this.targetGo);
				//scrollSystem.AttachScrollData(this.targetGo, null);
				this.targetGo = null;
			}
		}

		/// <summary>
		/// 更新内容
		/// </summary>
		/// <param name="refresh">表示强制刷新</param>
		public void UpdateContent(bool refresh = false)
		{
			if (isVisible)
			{
				if (this.targetGo == null)
				{
					this.targetGo = objectPool.Get();
					//scrollSystem.AttachScrollData(this.targetGo, this);
					refresh = true;
				}
				if (refresh)
				{
					var rectTransform = this.targetGo.transform as RectTransform;
					rectTransform.sizeDelta = new Vector2(this.width, this.height);
					this.scrollSystem.setItemContent(objectPool.prefabName, rectTransform, dataSource);
				}
				if (this.targetGo.transform.position != worldPosition)
				{
					this.targetGo.transform.position = worldPosition;
#if UNITY_EDITOR
					ShowGizmosBounds();
#endif
				}	
			}
		}

		public void ShowGizmosBounds()
		{
			if (scrollSystem.DrawGizmos)
			{
				Tools.DrawRectBounds(this.worldPosition, scrollSystem.contentTrans.lossyScale.x * width, scrollSystem.contentTrans.lossyScale.y * height, Color.red);
			}
		}

		/// <summary>
		/// 计算世界坐标位置，并且计算是否可见
		/// 这里只是模拟计算位置，不对预制体进行任何操作
		/// </summary>
		public void UpdatePos(uint frame)
		{
			if (frame > lastUpdateFrame)
			{
				lastUpdateFrame = frame;
				//根据contentTrans更新世界坐标
				this.worldPosition = Tools.GetUIPosByAnchoredPos(scrollSystem.contentTrans, this.anchoredPosition + scrollSystem.forceCenterOffset, DEFAULT_ANCHOR);
				//设置自己的RectBounds
				var lossyScale = scrollSystem.contentTrans.lossyScale;
				rectBounds.left = worldPosition.x - 0.5f * width * lossyScale.x;
				rectBounds.right = worldPosition.x + 0.5f * width * lossyScale.x;
				rectBounds.up = worldPosition.y + 0.5f * height * lossyScale.y;
				rectBounds.down = worldPosition.y - 0.5f * height * lossyScale.y;
				//判断是否可见
				isVisible = rectBounds.Overlaps(scrollSystem.scrollBounds);
			}
		}

		/// <summary>
		/// 设置位置
		/// </summary>
		public void SetAnchoredPosition(Vector2 position)
		{
			this.isPositionInited = true;
			position.y = -position.y;
			anchoredPosition = position;
		}

	}
}