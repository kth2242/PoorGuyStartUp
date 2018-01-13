using UnityEngine;
using System.Collections;

public class SpriteAnimator : MonoBehaviour
{
	/* AnimationTrigger class that is help for sequential animation */
	[System.Serializable]
	public class AnimationTrigger
	{
		public int frame;
		public string name; // variable to keep the method name to be triggered
	}

	/* Animation class that would hold animation to play */
	[System.Serializable]
	public class Animation
	{
		public string name;
		public int fps;
		public Sprite[] frames;

		public AnimationTrigger[] triggers;
	}

	public SpriteRenderer spriteRenderer; // variable to keep the sprite renderer reference
	public Animation[] animations; // variable to hold many animations in one SpriteAnimator class

	public bool playing { get; private set; } // variable to check if animation is playing
	public Animation currentAnimation { get; private set; } // variable to indicate the current animation
	public int currentFrame { get; private set; } // variable to indicate the current frame of the animation
	public bool loop { get; private set; } // variable to check whether animation would be looped or not

	public string playAnimationOnStart; // variable to hold the name of the start animation (name should be same as the name attributes in the Animation class

	void Awake()
	{
		if (!spriteRenderer)
			spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void OnEnable()
	{
		/* if there is starting animation, play start animation */
		if (playAnimationOnStart != "")
			Play(playAnimationOnStart);
	}

	void OnDisable()
	{
		playing = false;
		currentAnimation = null;
	}

	/* function to play the animation (recommended) */
	/* first parameter name should be same as the name attributes in the Animation class */
	public void Play(string name, bool loop = true, int startFrame = 0)
	{
		/* find the animation with given name */
		Animation animation = GetAnimation(name);
		if (animation != null )
		{
			/* if animation is not already playing */
			if (animation != currentAnimation)
			{
				/* play the animation */
				ForcePlay(name, loop, startFrame);
			}
		}
		else
		{
			/* if no animation matches the given name, warning message would be shown */
			Debug.LogWarning("could not find animation: " + name);
		}
	}

	/* function to play the animation (this function would be called in the Play function) */
	public void ForcePlay(string name, bool loop = true, int startFrame = 0)
	{
		/* find the animation with given name */
		Animation animation = GetAnimation(name);
		if (animation != null)
		{
			/* set attributes */
			this.loop = loop;
			currentAnimation = animation;
			playing = true;
			currentFrame = startFrame;
			spriteRenderer.sprite = animation.frames[currentFrame]; // The initial sprite to be drawn is defined here
			StopAllCoroutines();
			StartCoroutine(PlayAnimation(currentAnimation)); // Actual playing method
		}
	}

	public bool IsPlaying(string name)
	{
		return (currentAnimation != null && currentAnimation.name == name);
	}

	public Animation GetAnimation(string name)
	{
		/* This Animation class is declared above */
		/* check for all animations user have assigned */
		foreach (Animation animation in animations)
		{
			if (animation.name == name)
			{
				return animation;
			}
		}
		return null;
	}

	/* coroutine function to actual play animation */
	IEnumerator PlayAnimation(Animation animation)
	{
		float timer = 0f;
		float delay = 1f / (float)animation.fps;

		/* loop while animation looping signal is on
		   and current frame is not the end frame of the animation */
		/* animation.frames.Length -1 : decreasing by 1 code should exist because below while-loop call the NextFrame function and that function check the boundary of the frame number.
		   In NextFrame function, if current frame is out of the boundary and animation is not looping, current frame becomes the last frame of the animation.
		   After escaping from the NextFrame function, spriteRender would render the last frame and then possible to exit from the while-loop*/
		while (loop || currentFrame < animation.frames.Length-1)
		{
			while (timer < delay)
			{
				timer += Time.deltaTime;
				yield return 0f;
			}
			while (timer > delay)
			{
				timer -= delay;
				NextFrame(animation); // after assigned delay time is passed, change the frame to the next one
			}

			spriteRenderer.sprite = animation.frames[currentFrame]; // The sprite to be drawn is defined here
		}

		currentAnimation = null;
	}

	void NextFrame(Animation animation)
	{
		currentFrame++;

		/* for all animation trigger */
		foreach (AnimationTrigger animationTrigger in currentAnimation.triggers)
		{
			/* if animation trigger frame is same as the current frame (actually, next frame) */
			if (animationTrigger.frame == currentFrame)
			{
				/* trigger the method */
				gameObject.SendMessageUpwards(animationTrigger.name);
			}
		}

		/* if current frame (actually, next frame) is out of the boundary of the animation frame */
		if (currentFrame >= animation.frames.Length)
		{
			/* if animation looping signal is on, set the current frame as first frame,
			   otherwise set the current frame as last frame */
			if (loop)
				currentFrame = 0;
			else
				currentFrame = animation.frames.Length - 1;
		}
	}
}