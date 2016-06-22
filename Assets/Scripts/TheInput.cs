using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheInput : MonoBehaviour
{
    public int maxQueueSize = 10000;

    public static Vector3 leftVector = Vector3.left;
    public static Vector3 rightVector = Vector3.right;
    public static Vector3 upVector = Vector3.forward;
    public static Vector3 downVector = Vector3.back;

    public static Vector3[] indexedDirections =
    {
        Vector3.zero,
        upVector,
        downVector,
        leftVector,
        rightVector
    };

    public class InputData
    {
        public enum InputDataType
        {
            Movement,
            Fire,
            Jump
        }
        public InputDataType type = InputDataType.Movement;
        public Vector3 data;
    };

    Queue<InputData> queue = new Queue<InputData>();
    SwipeControl swipeControl;

    public Widget parentWidget = null;

	void Start ()
    {
        swipeControl = new SwipeControl();
        swipeControl.OnSwipe = OnSwipe;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(swipeControl != null)
            swipeControl.Update();

	    if (Input.GetKeyDown("up"))
        {
            PushMoveUp();
        }

        if (Input.GetKeyDown("down"))
        {
            PushMoveDown();
        }

        if (Input.GetKeyDown("left"))
        {
            PushMoveLeft();
        }

        if (Input.GetKeyDown("right"))
        {
            PushMoveRight();
        }
	}

    public void Push(InputData.InputDataType type, Vector3 data)
    {
        InputData ind = new InputData();
        ind.type = type;
        ind.data = data;

        queue.Enqueue(ind);
    }

    public void PushMoveLeft()
    {
        Push(InputData.InputDataType.Movement, leftVector);
    }

    public void PushMoveRight()
    {
        Push(InputData.InputDataType.Movement, rightVector);
    }

    public void PushMoveUp()
    {
        Push(InputData.InputDataType.Movement, upVector);
    }

    public void PushMoveDown()
    {
        Push(InputData.InputDataType.Movement, downVector);
    }

    public InputData Pop()
    {
        if(queue.Count > 0)
            return queue.Dequeue();
        return null;
    }
    public InputData Peek()
    {
        if (queue.Count > 0)
            return queue.Peek();
        return null;
    }

    public void Clear()
    {
        queue.Clear();
    }

    public bool IsMovingUp(InputData ind)
    {
        return ind.type == InputData.InputDataType.Movement && ind.data == upVector;
    }

    public bool IsMovingDown(InputData ind)
    {
        return ind.type == InputData.InputDataType.Movement && ind.data == downVector;
    }

    public bool IsMovingLeft(InputData ind)
    {
        return ind.type == InputData.InputDataType.Movement && ind.data == leftVector;
    }

    public bool IsMovingRight(InputData ind)
    {
        return ind.type == InputData.InputDataType.Movement && ind.data == rightVector;
    }

    void OnSwipe(Vector2 dir)
    {
        if(parentWidget != null)
        {
            if (parentWidget != Widget.capture) return;
        }

        if (dir.x < 0.0f)
            PushMoveLeft();

        if (dir.x > 0.0f)
            PushMoveRight();

        if (dir.y > 0.0f)
            PushMoveUp();

        if (dir.y < 0.0f)
            PushMoveDown();
    }
}

