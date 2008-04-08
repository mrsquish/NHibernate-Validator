using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using NHibernate.Validator.Engine;

namespace NHibernate.Validator.Interpolator
{
	[Serializable]
	public class DefaultMessageInterpolatorAggregator : IMessageInterpolator
	{
		private IDictionary<IValidator, DefaultMessageInterpolator> interpolators =
			new Dictionary<IValidator, DefaultMessageInterpolator>();

		//transient but repopulated by the object owing a reference to the interpolator
		[NonSerialized] private ResourceManager messageBundle;

		//transient but repopulated by the object owing a reference to the interpolator
		[NonSerialized] private ResourceManager defaultMessageBundle;

		private CultureInfo culture;

		public void Initialize(ResourceManager messageBundle, ResourceManager defaultMessageBundle, CultureInfo culture)
		{
			this.culture = culture;
			this.messageBundle = messageBundle;
			this.defaultMessageBundle = defaultMessageBundle;

			//useful when we deserialize
			foreach(DefaultMessageInterpolator interpolator in interpolators.Values)
			{
				interpolator.Initialize(messageBundle, defaultMessageBundle,culture);
			}
		}

		public string Interpolate<A>(string message, IValidator<A> validator, IMessageInterpolator defaultInterpolator)
			where A : Attribute
		{
			return Interpolate(message, (IValidator) validator, defaultInterpolator);
		}

		public string Interpolate(string message, IValidator validator, IMessageInterpolator defaultInterpolator)
		{
			DefaultMessageInterpolator defaultMessageInterpolator = interpolators[validator];

			if (defaultMessageInterpolator == null)
			{
				return message;
			}

			return defaultMessageInterpolator.Interpolate(message, validator, defaultInterpolator);
		}

		public void AddInterpolator(Attribute attribute, IValidator validator)
		{
			DefaultMessageInterpolator interpolator = new DefaultMessageInterpolator();
			interpolator.Initialize(messageBundle, defaultMessageBundle, culture);
			interpolator.Initialize(attribute, null);
			interpolators.Add(validator, interpolator);
		}

		public string GetAttributeMessage(IValidator validator)
		{
			DefaultMessageInterpolator defaultMessageInterpolator = interpolators[validator];

			string message = defaultMessageInterpolator != null
			                 	? defaultMessageInterpolator.GetAttributeMessage()
			                 	: null;

			if (message == null)
			{
				throw new AssertionFailure("Validator not registred to the messageInterceptorAggregator");
			}
			return message;
		}
	}
}