/*
 * Copyright (c) 2016 
 *
 * time:   2017-10-26
 * desc:   FP的序列化工具
 * author:  Zhonglin
 *
 */

using Newtonsoft.Json;
using MissQ;

/// <summary>
/// FP的多态序列化工具
/// </summary>
public class FPConverter : JsonConverter
{
	public override bool CanConvert(System.Type objectType)
	{
		return typeof(FP) == objectType;
	}

	public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		string vstr = reader.Value.ToString();
		FP result = 0;
		if (FPTools.TryParse(vstr, out result))
		{
			return result;
		}
		return null;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		FP v = (FP)value;
		writer.WriteRawValue(v.ToString());
	}
}
