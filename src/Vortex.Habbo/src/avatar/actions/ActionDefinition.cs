// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActionDefinition.as
// @see PRODUCTION-201611291003-338511768-Source-main/src/com/sulake/habbo/avatar/actions/ActionDefinition.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Actions;

/// <summary>
/// Defines a single avatar action parsed from XML.
/// Holds metadata (precedence, geometry, part sets), sub-types, parameters, and canvas offsets.
/// </summary>
/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActionDefinition.as
public class ActionDefinition : IActionDefinition
{
	private readonly string[] _prevents;
	private readonly bool _preventHeadTurn;
	private readonly Dictionary<string, ActionType> _types;
	private readonly string _defaultParameterValue = "";

	// Two-level map: outer key = size string (e.g. "h", "sh"),
	// inner key = direction int, value = double[3] (x, y, z).
	private Dictionary<string, Dictionary<int, double[]>>? _canvasOffsets;

	/// @see ActionDefinition.as::ActionDefinition
	public ActionDefinition(XElement xml)
	{
		_types = new Dictionary<string, ActionType>();
		Params = new Dictionary<string, string>();

		Id = (string?)xml.Attribute("id") ?? "";
		State = (string?)xml.Attribute("state") ?? "";
		Precedence = int.TryParse((string?)xml.Attribute("precedence"), out int prec) ? prec : 0;
		ActivePartSet = (string?)xml.Attribute("activepartset") ?? "";
		AssetPartDefinition = (string?)xml.Attribute("assetpartdefinition") ?? "";
		Lay = (string?)xml.Attribute("lay") ?? "";
		GeometryType = (string?)xml.Attribute("geometrytype") ?? "";

		// AS3: Boolean(parseInt(param1.@main)) — parseInt then truthy (0=false, nonzero=true)
		IsMain = (int.TryParse((string?)xml.Attribute("main"), out int mainVal) && mainVal != 0);
		IsDefault = (int.TryParse((string?)xml.Attribute("isdefault"), out int defVal) && defVal != 0);
		IsAnimation = (int.TryParse((string?)xml.Attribute("animation"), out int animVal) && animVal != 0);

		StartFromFrameZero = (string?)xml.Attribute("startfromframezero") == "true";
		_preventHeadTurn = (string?)xml.Attribute("preventheadturn") == "true";

		string preventsStr = (string?)xml.Attribute("prevents") ?? "";

		if (preventsStr != "")
		{
			_prevents = preventsStr.Split(',');
		}
		else
		{
			_prevents = [];
		}

		// Parse <param> children
		foreach (XElement paramEl in xml.Elements("param"))
		{
			string paramId = (string?)paramEl.Attribute("id") ?? "";
			string paramValue = (string?)paramEl.Attribute("value") ?? "";

			if (paramId == "default")
			{
				_defaultParameterValue = paramValue;
			}
			else
			{
				Params[paramId] = paramValue;
			}
		}

		// Parse <type> children
		foreach (XElement typeEl in xml.Elements("type"))
		{
			string typeId = (string?)typeEl.Attribute("id") ?? "";
			_types[typeId] = new ActionType(typeEl);
		}
	}

	/// @see ActionDefinition.as::setOffsets
	public void SetOffsets(string size, int direction, double[] offsets)
	{
		_canvasOffsets ??= new Dictionary<string, Dictionary<int, double[]>>();

		if (!_canvasOffsets.TryGetValue(size, out Dictionary<int, double[]>? value))
		{
			value = new Dictionary<int, double[]>();

			_canvasOffsets[size] = value;
		}

		value[direction] = offsets;
	}

	/// @see ActionDefinition.as::getOffsets
	public double[]? GetOffsets(string size, int direction)
	{
		if (_canvasOffsets == null)
		{
			return null;
		}

		if (!_canvasOffsets.TryGetValue(size, out Dictionary<int, double[]>? innerMap))
		{
			return null;
		}

		return innerMap.GetValueOrDefault(direction);
	}

	/// @see ActionDefinition.as::getParameterValue
	public string GetParameterValue(string param)
	{
		if (param == "")
		{
			return "";
		}

		return Params.GetValueOrDefault(param, _defaultParameterValue);

	}

	/// @see ActionDefinition.as::getTypePrevents (PRODUCTION: _Str_1889)
	private string[] GetTypePrevents(string param)
	{
		if (param == "")
		{
			return [];
		}

		if (_types.TryGetValue(param, out ActionType? actionType))
		{
			return actionType.Prevents;
		}

		return [];
	}

	/// @see ActionDefinition.as::get id
	public string Id { get; }

	/// @see ActionDefinition.as::get state
	public string State { get; }

	/// @see ActionDefinition.as::get precedence
	public int Precedence { get; }

	/// @see ActionDefinition.as::get activePartSet
	public string ActivePartSet { get; }

	/// @see ActionDefinition.as::get isMain
	public bool IsMain { get; }

	/// @see ActionDefinition.as::get isDefault
	public bool IsDefault { get; }

	/// @see ActionDefinition.as::get assetPartDefinition
	public string AssetPartDefinition { get; }

	/// @see ActionDefinition.as::get lay
	public string Lay { get; }

	/// @see ActionDefinition.as::get geometryType
	public string GeometryType { get; }

	/// @see ActionDefinition.as::get isAnimation
	public bool IsAnimation { get; }

	/// @see ActionDefinition.as::get startFromFrameZero
	public bool StartFromFrameZero { get; }

	/// @see ActionDefinition.as::getPrevents
	public string[] GetPrevents(string param = "")
	{
		// AS3: var_3714.concat(getTypePrevents(param1)) — always returns a new array
		string[] typePrevents = GetTypePrevents(param);
		string[] result = new string[_prevents.Length + typePrevents.Length];
		_prevents.CopyTo(result, 0);
		typePrevents.CopyTo(result, _prevents.Length);
		return result;
	}

	/// @see ActionDefinition.as::getPreventHeadTurn
	public bool GetPreventHeadTurn(string param = "")
	{
		if (param == "")
		{
			return _preventHeadTurn;
		}

		if (_types.TryGetValue(param, out ActionType? actionType))
		{
			return actionType.PreventHeadTurn;
		}

		return _preventHeadTurn;
	}

	/// @see ActionDefinition.as::isAnimated
	public bool IsAnimated(string param)
	{
		if (param == "")
		{
			return true;
		}

		return !_types.TryGetValue(param, out ActionType? actionType) || actionType.IsAnimated;

	}

	/// @see ActionDefinition.as::get params
	public Dictionary<string, string> Params { get; }

	/// @see ActionDefinition.as::toString
	public override string ToString()
	{
		return "[ActionDefinition]\n"
			   + "id:           " + Id + "\n"
			   + "state:        " + State + "\n"
			   + "main:         " + IsMain + "\n"
			   + "default:      " + IsDefault + "\n"
			   + "geometry:     " + GeometryType + "\n"
			   + "precedence:   " + Precedence + "\n"
			   + "activepartset:" + ActivePartSet + "\n"
			   + "activepartdef:" + AssetPartDefinition;
	}
}
