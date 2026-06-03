namespace Vortex.Habbo.Avatar.Enum;

/// <summary>
/// Avatar expression, gesture, and posture string constants with lookup helpers.
/// </summary>
/// @see com.sulake.habbo.avatar.enum.class_3584
public static class AvatarExpressionEnum
{
    public const string SIGN = "sign";
    public const string SLEEP = "Sleep";
    public const string CARRY_OBJECT = "cri";
    public const string USE_OBJECT = "usei";
    public const string EFFECT = "fx";
    public const string TALK = "talk";
    public const string GESTURE = "gest";
    public const string EXPRESSION = "expression";
    public const string VOTE = "vote";
    public const string DANCE = "dance";
    public const string TYPING = "typing";
    public const string MUTED = "muted";
    public const string PLAYING_GAME = "playing_game";
    public const string GUIDE_STATUS = "guide";
    public const string RESPECT = "respect";

    public const string EXPRESSION_JUMP = "dance";
    public const string EXPRESSION_WAVE = "wave";
    public const string EXPRESSION_BLOW_A_KISS = "blow";
    public const string EXPRESSION_LAUGH = "laugh";
    public const string EXPRESSION_CRY = "cry";
    public const string EXPRESSION_IDLE = "idle";
    public const string EXPRESSION_SNOWBOARD_OLLIE = "sbollie";
    public const string EXPRESSION_SNOWBORD_360 = "sb360";
    public const string EXPRESSION_RIDE_JUMP = "ridejump";

    public const string GESTURE_SMILE = "sml";
    public const string GESTURE_AGGRAVATED = "agr";
    public const string GESTURE_SURPRISED = "srp";
    public const string GESTURE_SAD = "sad";

    public const string PET_GESTURE_JOY = "joy";
    public const string PET_GESTURE_CRAZY = "crz";
    public const string PET_GESTURE_TONGUE = "tng";
    public const string PET_GESTURE_BLINK = "eyb";
    public const string PET_GESTURE_MISERABLE = "mis";
    public const string PET_GESTURE_PUZZLED = "puz";

    public const string POSTURE = "posture";
    public const string POSTURE_STAND = "std";
    public const string POSTURE_SIT = "sit";
    public const string POSTURE_WALK = "mv";
    public const string POSTURE_LAY = "lay";
    public const string POSTURE_SWIM = "swim";
    public const string POSTURE_FLOAT = "float";
    public const string POSTURE_SNOWWAR_RUN = "swrun";
    public const string POSTURE_SNOWWAR_DIE_FRONT = "swdiefront";
    public const string POSTURE_SNOWWAR_DIE_BACK = "swdieback";
    public const string POSTURE_SNOWWAR_PICK = "swpick";
    public const string POSTURE_SNOWWAR_THROW = "swthrow";

    private static readonly string[] GESTURE_MAP =
        ["", "sml", "agr", "srp", "sad", "joy", "crz", "tng", "eyb", "mis", "puz"];

    private static readonly string[] EXPRESSION_MAP =
        ["", "wave", "blow", "laugh", "cry", "idle", "dance", "respect", "sbollie", "sb360", "ridejump"];

    public static int GetExpressionTime(int expressionId)
    {
        return (expressionId - 1) switch
        {
            0 => 5000, // wave
            1 => 1400, // blow
            2 => 2000, // laugh
            3 => 2000, // cry
            4 => 0,    // idle
            5 => 700,  // dance
            6 => 2000, // respect
            7 => 1500, // sbollie
            8 => 1500, // sb360
            9 => 1500, // ridejump
            _ => 0,
        };
    }

    public static int GetExpressionId(string expression)
    {
        return System.Array.IndexOf(EXPRESSION_MAP, expression);
    }

    public static string GetExpression(int id)
    {
        if (id >= 0 && id < EXPRESSION_MAP.Length)
        {
            return EXPRESSION_MAP[id];
        }
        return "";
    }

    public static int GetGestureId(string gesture)
    {
        return System.Array.IndexOf(GESTURE_MAP, gesture);
    }

    public static string GetGesture(int id)
    {
        if (id >= 0 && id < GESTURE_MAP.Length)
        {
            return GESTURE_MAP[id];
        }
        return "";
    }
}
