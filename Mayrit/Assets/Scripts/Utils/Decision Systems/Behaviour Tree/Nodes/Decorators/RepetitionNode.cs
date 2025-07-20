using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RepetitionNode is a node that continues running its only child X times.
/// </summary>
public class RepetitionNode<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    private int _repetitions;
    private int _currentRepetition = 0;
    private Node<TController> _child; // Make sure we have a reference to the child

    public RepetitionNode(TController controller, int repetitions, Node<TController> child, int priority = 0)
    : base(controller, repetitions.ToString() + " repetions", priority)
    {
        _repetitions = repetitions;
        AddChild(child); // Use the AddChild method to set the child
        _child = children[0]; // Store a direct reference for easier access
    }

    public override Status UpdateNode()
    {
        if (_child == null)
        {
            return Status.Failure; // If there is no child, the node fails
        }

        while (_currentRepetition < _repetitions)
        {
            Status childStatus = _child.UpdateNode();

            if (childStatus == Status.Success)
            {
                _currentRepetition++;
                if (_currentRepetition == _repetitions)
                {
                    return Status.Success; // We have reached the number of repetitions
                }
                // If we haven't finished, the node stays in Running for the next tick
                return Status.Running;
            }
            else if (childStatus == Status.Running)
            {
                return Status.Running; // The child is still running
            }
            else // childStatus == Status.Failure
            {
                return Status.Failure; // If the child fails, the repetition node also fails
            }
        }

        // If for some reason we get here after the loop ends (which shouldn't happen
        // in a normal execution), we return success as the repetitions are complete.
        return Status.Success;
    }

    public override void Reset()
    {
        base.Reset();
        _currentRepetition = 0; // Reset the counter when the node is reset
    }
}