/*
 * Copyright (c) 2016 
 *
 * time:   2017-10-26
 * desc:   Vector3的多态序列化工具, 为了避免normalized的递归序列化
 * author:  Zhonglin
 *
 */

using Newtonsoft.Json;
using MissQ;

/// <summary>
/// TSVector的多态序列化工具, 为了避免normalized的递归序列化
/// </summary>
public class TSVectorConverter : JsonConverter
{
	class SimpleVector
	{
		public float x, y, z;
	}

	public override bool CanConvert(System.Type objectType)
	{
		return typeof(TSVector) == objectType;
	}

	public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		return null;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		TSVector v = (TSVector)value;
		SimpleVector sv = new SimpleVector(){x = v.x.AsFloat(), y = v.y.AsFloat(), z = v.z.AsFloat()};
		serializer.Serialize(writer, sv);
	}
}
