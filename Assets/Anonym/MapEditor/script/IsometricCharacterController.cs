using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
	public enum InGameDirection
	{
		Jump_Move = 0,	
		Right_Move = 1,
		RD_Move = 2,
		RD_Rotate = -1 * RD_Move,
		Down_Move = 3,
		LD_Move = 4,
		LD_Rotate = -1 * LD_Move,
		Left_Move = 5,
		LT_Move = 6,
		LT_Rotate = -1 * LT_Move,
		Top_Move = 7,
		RT_Move = 8,
		RT_Rotate = -1 * RT_Move,
		Dash = 9,
	}
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CharacterController))]
	[ExecuteInEditMode][DefaultExecutionOrder(1)]
	public class IsometricCharacterController : MonoBehaviour {

		[SerializeField]		Queue<InGameDirection> DirQ = new Queue<InGameDirection>();
		[SerializeField]		int iMaxQSize = 2;
		[SerializeField]		InGameDirection LastDirection = InGameDirection.Top_Move;

		[SerializeField]		CharacterController CC;
		[SerializeField]		bool bOnMoving = false;
		[SerializeField]		bool bDashing = false;
		[HideInInspector]		bool bOnJumpingBoost {get{return fJumpingPowerDurationInst > 0;}}
		[SerializeField]		float fJumpingPower = 0.1f;
		[SerializeField]		float fJumpingPowerDuration = 0.2f;
		[SerializeField]		float fJumpingPowerDurationInst = 0;

		[SerializeField]		Vector3 vDestination;
		[SerializeField]		string FloorLayerMask = "Default";
		[SerializeField]		float fMaxDropHeight = 100f;
	
		[SerializeField]		bool bFreeJumpMode = true;
		[SerializeField]		float fMaxDashInputInterval = 0.33f;
		float fLastInputTime = 0;

		[SerializeField]		bool bUseCCSize = false;
		[SerializeField]		bool bRotateToDirection = false;

		[SerializeField]        Vector2 CCSize;

		[SerializeField]        IsometircSortingOrder _so = null;
		[HideInInspector]
        public IsometircSortingOrder sortingOrder{get{
            return _so != null ? _so : _so = GetComponent<IsometircSortingOrder>();
        }}

        public int SortingOrder_Adjustment()
        {
			// 땅에서 떨어진 정도가 CCSize.x 이상일 때 CCSize.y, CCSize.x 이하일 때 CCSize.x ~ CCSize.y 리턴
			float fXweight = 0f;
			//if ((CC.collisionFlags & CollisionFlags.Below) == 0)
			{				
				RaycastHit _hit;
				float fOffset = CC.height * 0.5f + CC.skinWidth;
				if (Physics.Raycast(CC.transform.position + CC.center, Vector3.down, out _hit,
						CCSize.x + fOffset, 1 << LayerMask.NameToLayer(FloorLayerMask)))
				{
					fXweight = Mathf.Lerp(CCSize.x, 0f, 
						(_hit.distance - fOffset * 0.25f) / CCSize.x);
				}
			}
			Vector3 iv3Resolution = IsoMap.instance.fResolutionOfIsometric;
			return Mathf.RoundToInt(fXweight * CCSize.x * Mathf.Min(iv3Resolution.z, iv3Resolution.x) + 
				(1f - fXweight) * CCSize.y * iv3Resolution.y);
        }
		
	#region MoveFunction
		public void EnQueueTo(InGameDirection dir)
		{
			if (bOnMoving && LastDirection == dir
				&& (Time.time - fLastInputTime < fMaxDashInputInterval))
			{
				vMoveTo(InGameDirection.Dash);
			}
			else if (DirQ.Count < iMaxQSize)
			{
				DirQ.Enqueue(dir);
			}
			fLastInputTime = Time.time;
		}
		void vMoveTo(InGameDirection dir)
		{		
			bool bMove = dir > 0;
			if (!bMove)
			{
				dir = (InGameDirection) (-1 * (int)dir);
			}

			if(dir.Equals(InGameDirection.Dash) || (!bOnMoving && (CC.isGrounded || bFreeJumpMode)))
			{
				if (!bFreeJumpMode && dir.Equals(InGameDirection.Jump_Move))
				{
					//CC.SimpleMove(Vector3.down * 0.1f); (CC.collisionFlags & CollisionFlags.Below) != 0 || 
					fJumpingPowerDurationInst = fJumpingPowerDuration;
					//_action.Play_Action(Action.Type.Jump);
					//return;
					dir = LastDirection;
				}

				if (bDashing = dir.Equals(InGameDirection.Dash))
					dir = LastDirection;
				else
					LastDirection = dir;

				float fAngle = 0f, fLength = 1f;
				Vector3 v3Tmp = Vector3.forward;
				Vector3 v3LocalYZero = new Vector3(CC.transform.localPosition.x, 0, CC.transform.localPosition.z);
				// if (bDashing)
				// {
				// 	if (Vector3.Distance(vDestination, v3LocalYZero) > 0.5f)
				// 		fLength *= 2f;
				// }

				if (dir.Equals(InGameDirection.Left_Move) || dir.Equals(InGameDirection.Top_Move)
					|| dir.Equals(InGameDirection.Right_Move) || dir.Equals(InGameDirection.Down_Move))
					fLength *= 1.414f;
				
				fAngle = (int) dir * 45f;
				if (bRotateToDirection)
					CC.transform.localEulerAngles = new Vector3(0, fAngle, 0);

				if (bMove)
				{
					v3Tmp = Quaternion.AngleAxis(fAngle, Vector3.up) * v3Tmp * fLength;
					v3Tmp = vDestination + v3Tmp;
					v3Tmp.y = CC.transform.position.y;// - fMaxDropHeight;
					//Debug.Log(v3);
					bMove = Physics.Raycast(v3Tmp, Vector3.down, 
						fMaxDropHeight, 1 << LayerMask.NameToLayer(FloorLayerMask));					
				}

				vDestination = bMove ? v3Tmp : (bDashing ? vDestination : v3LocalYZero);
				vDestination.Set(Mathf.RoundToInt(vDestination.x), 0, Mathf.RoundToInt(vDestination.z));

				if (Vector3.Distance(vDestination, v3LocalYZero) >= CC.minMoveDistance)
					bOnMoving = true;

				// if (bDashing)
				// 	_action.Play_Action(Action.Type.Dash);
				// else if (!bOnJumpingBoost)
				// 	_action.Play_Action(Action.Type.Walk);
			}
			else
			{
				EnQueueTo(dir);
			}
		}
		[SerializeField]
		float fMovingSpeed = 2f;
		[SerializeField]
		float fDashSpeedMultiplier = 4f;
		[SerializeField]
		float fGridTolerance = 0.05f;
		float fMinMovement = 0f;

		public void Jump()
		{
			if (bFreeJumpMode)
			{
				if (!CC.isGrounded)
					CC.Move(Vector3.down * 1.25f * CC.minMoveDistance);

				if (CC.isGrounded)
				{
					fJumpingPowerDurationInst = fJumpingPowerDuration;
					//_action.Play_Action(Action.Type.Jump);
				}
			}
			else
				EnQueueTo(InGameDirection.Jump_Move);
	
			return;
		}
	#endregion
	#region FightFuntion
		
	#endregion
		void Start()
		{
			if (CC == null)
			{
				CC = gameObject.GetComponent<CharacterController>();
			}
			if (CCSize.Equals(Vector2.zero) && bUseCCSize)
			{
				CCSize = new Vector2(Mathf.Max(Grid.fGridTolerance, CC.radius * 2f), 
					Mathf.Max(Grid.fGridTolerance, CC.height + CC.center.y));
			}
			// if (_action == null)
			// 	_action = gameObject.GetComponent<Action>();			

			fMinMovement = Mathf.Min(CC.minMoveDistance, fGridTolerance);
			fMinMovement *= fMinMovement;

			vDestination.Set(Mathf.RoundToInt(CC.transform.localPosition.x), 
						0, Mathf.RoundToInt(CC.transform.localPosition.z));
		}
		void Update()
		{
			if (Application.isPlaying)
			{
				InputProcess();
				update_Position();	
			}
			if (transform.hasChanged && sortingOrder != null)
			{
				sortingOrder.iExternAdd = SortingOrder_Adjustment();
			}
		}

		void update_Position()
		{
			Vector3 vMovementTmp = Vector3.zero;

			if (fJumpingPowerDurationInst > 0f)
			{
				fJumpingPowerDurationInst = Mathf.Max(0f, fJumpingPowerDurationInst - Time.deltaTime);
				vMovementTmp += Vector3.up * fJumpingPower;
			}
			else
			{
				vMovementTmp += Physics.gravity * Time.deltaTime;
			}

			if (bOnMoving)
			{
				Vector3 vLengthToDestination = vDestination - CC.transform.localPosition;
				vLengthToDestination.y = 0;
				
				if (vLengthToDestination.sqrMagnitude <= fMinMovement)
				{
					arrival();
				}
				else
				{
					vMovementTmp += Vector3.MoveTowards(Vector3.zero, vLengthToDestination, 
						(bDashing ? fDashSpeedMultiplier : 1f) * fMovingSpeed * Time.deltaTime);
				}
			}
			else if (DirQ.Count > 0)
			{
				vMoveTo(DirQ.Dequeue());
			}
			
			if (!vMovementTmp.Equals(Vector3.zero))
			{
				CC.Move(vMovementTmp);
				if ((CC.collisionFlags & CollisionFlags.Sides) != 0)
				{
					vDestination.Set(Mathf.RoundToInt(CC.transform.localPosition.x), 
						0, Mathf.RoundToInt(CC.transform.localPosition.z));
					arrival();				
				}
			}
		}

		void arrival()
		{
			bOnMoving = bDashing = false;
		}

		void InputProcess()
		{
			bool bShifted = Input.GetKey(KeyCode.LeftShift);
			if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			{				
				EnQueueTo(bShifted ? InGameDirection.LT_Rotate : InGameDirection.LT_Move);
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				EnQueueTo(bShifted ? InGameDirection.RD_Rotate : InGameDirection.RD_Move);
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				EnQueueTo(bShifted ? InGameDirection.RT_Rotate : InGameDirection.RT_Move);
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				EnQueueTo(bShifted ? InGameDirection.LD_Rotate : InGameDirection.LD_Move);
			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				Jump();
			}
		}
	}
}