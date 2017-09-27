﻿using Models.Core;
using Models.WholeFarm.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Models.WholeFarm;
using Models.WholeFarm.Groupings;
using System.ComponentModel.DataAnnotations;

namespace Models.WholeFarm.Activities
{
	/// <summary>Ruminant herd cost </summary>
	/// <summary>This activity will arrange payment of a herd expense such as vet fees</summary>
	[Serializable]
	[ViewName("UserInterface.Views.GridView")]
	[PresenterName("UserInterface.Presenters.PropertyPresenter")]
	[ValidParent(ParentType = typeof(WFActivityBase))]
	[ValidParent(ParentType = typeof(ActivitiesHolder))]
	[ValidParent(ParentType = typeof(ActivityFolder))]
	public class RuminantActivityHerdCost : WFRuminantActivityBase, IValidatableObject
	{
		/// <summary>
		/// Amount payable
		/// </summary>
		[Description("Amount payable")]
        [Required, Range(0, double.MaxValue, ErrorMessage = "Value must be a greter than or equal to 0")]
        public double Amount { get; set; }

		/// <summary>
		/// Payment style
		/// </summary>
		[System.ComponentModel.DefaultValueAttribute(AnimalPaymentStyleType.perHead)]
		[Description("Payment style")]
        [Required]
        public AnimalPaymentStyleType PaymentStyle { get; set; }

		/// <summary>
		/// name of account to use
		/// </summary>
		[Description("Name of account to use")]
        [Required]
        public string AccountName { get; set; }

        /// <summary>
        /// Validate object
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            switch (PaymentStyle)
            {
                case AnimalPaymentStyleType.Fixed:
                case AnimalPaymentStyleType.perHead:
                case AnimalPaymentStyleType.perAE:
                    break;
                default:
                    string[] memberNames = new string[] { "PaymentStyle" };
                    results.Add(new ValidationResult("Payment style " + PaymentStyle.ToString() + " is not supported", memberNames));
                    break;
            }
            return results;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RuminantActivityHerdCost()
		{
			this.SetDefaults();
		}

        /// <summary>An event handler to allow us to initialise ourselves.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("WFInitialiseActivity")]
        private void OnWFInitialiseActivity(object sender, EventArgs e)
        {
            this.InitialiseHerd(true);
		}

		/// <summary>
		/// Method to determine resources required for this activity in the current month
		/// </summary>
		/// <returns>List of required resource requests</returns>
		public override List<ResourceRequest> GetResourcesNeededForActivity()
		{
			ResourceRequestList = new List<ResourceRequest>();

			if (this.TimingOK)
			{
				double amountNeeded = 0;
                List<Ruminant> herd = this.CurrentHerd();
				switch (PaymentStyle)
				{
					case AnimalPaymentStyleType.Fixed:
						amountNeeded = Amount;
						break;
					case AnimalPaymentStyleType.perHead:
						amountNeeded = Amount*herd.Count();
						break;
					case AnimalPaymentStyleType.perAE:
						amountNeeded = Amount * herd.Sum(a => a.AdultEquivalent);
						break;
					default:
                        break;
				}

				if (amountNeeded == 0) return ResourceRequestList;

				// determine breed
				string BreedName = "Multiple breeds";
				List<string> breeds = herd.Select(a => a.Breed).Distinct().ToList();
				if(breeds.Count==1)
				{
					BreedName = breeds[0];
				}

				ResourceRequestList.Add(new ResourceRequest()
				{
					AllowTransmutation = false,
					Required = amountNeeded,
					ResourceType = typeof(Finance),
					ResourceTypeName = this.AccountName,
					ActivityModel = this,
					Reason = BreedName
				}
				);
			}
			return ResourceRequestList;
		}

		/// <summary>
		/// Method used to perform activity if it can occur as soon as resources are available.
		/// </summary>
		public override void DoActivity()
		{
            return;
		}

		/// <summary>
		/// Method to determine resources required for initialisation of this activity
		/// </summary>
		/// <returns></returns>
		public override List<ResourceRequest> GetResourcesNeededForinitialisation()
		{
			return null;
		}

		/// <summary>
		/// Method used to perform initialisation of this activity.
		/// This will honour ReportErrorAndStop action but will otherwise be preformed regardless of resources available
		/// It is the responsibility of this activity to determine resources provided.
		/// </summary>
		public override void DoInitialisation()
		{
			return;
		}

		/// <summary>
		/// Resource shortfall event handler
		/// </summary>
		public override event EventHandler ResourceShortfallOccurred;

		/// <summary>
		/// Shortfall occurred 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnShortfallOccurred(EventArgs e)
		{
			if (ResourceShortfallOccurred != null)
				ResourceShortfallOccurred(this, e);
		}

		/// <summary>
		/// Resource shortfall occured event handler
		/// </summary>
		public override event EventHandler ActivityPerformed;

		/// <summary>
		/// Shortfall occurred 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnActivityPerformed(EventArgs e)
		{
			if (ActivityPerformed != null)
				ActivityPerformed(this, e);
		}

    }
}
