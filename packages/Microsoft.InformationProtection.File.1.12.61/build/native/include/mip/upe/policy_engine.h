/*
 *
 * Copyright (c) Microsoft Corporation.
 * All rights reserved.
 *
 * This code is licensed under the MIT License.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files(the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions :
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */
/**
 * @brief This file contains the PolicyEngine class which includes the PolicyEngine::Settings class.
 * 
 * @file policy_engine.h
 */

#ifndef API_MIP_UPE_POLICY_ENGINE_H_
#define API_MIP_UPE_POLICY_ENGINE_H_

#include <memory>
#include <string>
#include <vector>

#include "mip/common_types.h"
#include "mip/error.h"
#include "mip/mip_namespace.h"
#include "mip/upe/action.h"
#include "mip/upe/content_label.h"
#include "mip/upe/execution_state.h"
#include "mip/upe/label.h"
#include "mip/upe/policy_handler.h"
#include "mip/upe/sensitivity_types_rule_package.h"

MIP_NAMESPACE_BEGIN

/**
 * @brief This class provides an interface for all engine functions.
 */
class PolicyEngine {
public:
  /**
   * @brief Defines the settings associated with a PolicyEngine. 
   */
  class Settings {
  public:
    /**
     * @brief PolicyEngine::Settings constructor for loading an existing engine.
     *
     * @param engineId Set it to the unique engine ID generated by AddEngineAsync or one self-generated. When loading an
     * existing engine, reuse the ID else a new engine will be created.
     * @param authDelegate The authentication delegate used by the SDK to acquire authentication tokens, will override the 
     * PolicyProfile::Settings::authDelegate if both provided
     * @param clientData customizable client data that can be stored with the engine when unloaded, can be retrieved from
     * a loaded engine.
     * @param locale engine localizable output will be provided in this locale.
     * @param Optional flag indicating when the engine is loaded should load also custom sensitivity types, 
     * if true OnPolicyChange Observer on the profile will be invoked on updates to custom sensitivity types as well as policy changes.
     * if false ListSensitivityTypes call will always return an empty list.
     */
    Settings(
        const std::string& engineId,
        const std::shared_ptr<AuthDelegate>& authDelegate,
        const std::string& clientData,
        const std::string& locale = "",
        bool loadSensitivityTypes = false)
        : mEngineId(engineId),
          mAuthDelegate(authDelegate),
          mClientData(clientData),
          mLocale(locale),
          mIsLoadSensitivityTypesEnabled(loadSensitivityTypes) {
      if (mLocale.compare("") == 0) {
        mLocale = "en-US";
      }
    }

    /**
     * @brief PolicyEngine::Settings constructor for creating a new engine.
     *
     * @param identity Identity info of the user associated with the new engine.
     * @param authDelegate The authentication delegate used by the SDK to acquire authentication tokens, will override the 
     * PolicyProfile::Settings::authDelegate if both provided
     * @param clientData customizable client data that can be stored with the engine when unloaded, can be retrieved from
     * a loaded engine.
     * @param locale engine localizable output will be provided in this locale.
     * @param Optional flag indicating when the engine is loaded should load also custom sensitivity types, 
     * if true OnPolicyChange Observer on the profile will be invoked on updates to custom sensitivity types as well as policy changes.
     * if false ListSensitivityTypes call will always return an empty list.
     */
    Settings(
        const Identity& identity,
        const std::shared_ptr<AuthDelegate>& authDelegate,
        const std::string& clientData,
        const std::string& locale = "",
        bool loadSensitivityTypes = false)
        : mIdentity(identity),
          mAuthDelegate(authDelegate),
          mClientData(clientData),
          mLocale(locale),
          mIsLoadSensitivityTypesEnabled(loadSensitivityTypes) {
      if (mLocale.compare("") == 0) {
        mLocale = "en-US";
      }
    }

    /**
     * @brief Get the engine ID.
     * 
     * @return a unique string identifying the engine.
     */
    const std::string& GetEngineId() const { return mEngineId; }

    /**
     * @brief Set the engine ID.
     * 
     * @param id engine ID.
     */
    void SetEngineId(const std::string& id) { mEngineId = id; }

    /**
     * @brief Get the Identity object.
     * 
     * @return a reference to the identity in the settings object.
     * @see mip::Identity
     */
    const Identity& GetIdentity() const { return mIdentity; }

    /**
     * @brief Set the Identity object.
     * 
     * @param identity the unique identity of a user.
     * @see mip::Identity
     */
    void SetIdentity(const Identity& identity) { mIdentity = identity; }

    /**
     * @brief Get the Client Data set in the settings.
     * 
     * @return a string of data specified by the client.
     */
    const std::string& GetClientData() const { return mClientData; }

    /**
     * @brief Set the Client Data string. 
     * 
     * @param clientData user specified data.
     */
    void SetClientData(const std::string& clientData) { mClientData = clientData; }

    /**
     * @brief Get the Locale set in the settings.
     * 
     * @return the locale.
     */
    const std::string& GetLocale() const { return mLocale; }

    /**
     * @brief Set the custom settings, used for feature gating and testing.
     * 
     * @param customSettings List of name/value pairs.
     */
    void SetCustomSettings(const std::vector<std::pair<std::string, std::string>>& customSettings) {
      mCustomSettings = customSettings;
    }

    /**
     * @brief Get the custom settings, used for feature gating and testing.
     * 
     * @return List of name/value pairs.
     */
    const std::vector<std::pair<std::string, std::string>>& GetCustomSettings() const {
      return mCustomSettings;
    }

    /**
     * @brief Set the session ID, used for client defined telemetry 
     * and to make it easier to correlate application events with the corresponding policy service REST requests. 
     * 
     * @param sessionId An identifier (usually specified as a GUID) to uniquely identify this operation.
     */
    void SetSessionId(const std::string& sessionId) {
      mSessionId = sessionId;
    }

    /**
     * @brief Get the session ID, a unique identifier.
     * 
     * @return the session ID.
     */
    const std::string& GetSessionId() const {
      return mSessionId;
    }

    /**
     * @brief Get the the flag indicating if load sensitivity labels is enabled.
     * 
     * @return true if enabled else false.
     */
    bool IsLoadSensitivityTypesEnabled() const {
      return mIsLoadSensitivityTypesEnabled;
    }

    /**
    * @brief Optionally sets the target cloud
    * 
    * @param cloud Cloud
    * 
    * @note If cloud is not specified, then it will default to commercial cloud.
    */
    void SetCloud(Cloud cloud) {
      mCloud = cloud;
    }

    /**
    * @brief Gets the target cloud used by all service requests
    * 
    * @return cloud
    */
    Cloud GetCloud() const {
      return mCloud;
    }

    /**
    * @brief Optionally sets the target diagnostic region
    * 
    * @param dataBoundary Data boundary region
    * 
    * @note If dataBoundary is not specified, then it will default to global diagnostic region.
    */
    void SetDataBoundary(DataBoundary dataBoundary) {
      mDataBoundary = dataBoundary;
    }

    /**
    * @brief Gets the data boundary region
    * 
    * @return DataBoundary
    */
    DataBoundary GetDataBoundary() const {
      return mDataBoundary;
    }

   /**
    * @brief Sets the cloud endpoint base URL for custom cloud
    * 
    * @param cloudEndpointBaseUrl the base URL used by all service requests (for example, "https://dataservice.protection.outlook.com")
    * 
    * @note This value will only be read and must be set for Cloud = Custom
    */
    void SetCloudEndpointBaseUrl(const std::string& cloudEndpointBaseUrl) {
      mCloudEndpointBaseUrl = cloudEndpointBaseUrl;
    }

    /**
    * @brief Gets the cloud base URL used by all service requests, if specified
    * 
    * @return base URL
    */
    const std::string& GetCloudEndpointBaseUrl() const {
      return mCloudEndpointBaseUrl;
    }

    /**
     * @brief Sets the delegated user
     *
     * @param delegatedUserEmail the delegation email.
     * 
     * @note A delegated user is specified when the authenticating user/application is acting on behalf of another user
     */
    void SetDelegatedUserEmail(const std::string& delegatedUserEmail) { mDelegatedUserEmail = delegatedUserEmail; }

    /**
     * @brief Gets the delegated user
     * 
     * @return Delegated user
     * 
     * @note A delegated user is specified when the authenticating user/application is acting on behalf of another user
     */
    const std::string& GetDelegatedUserEmail() const { return mDelegatedUserEmail; }

    /**
     * @brief Sets the label filter
     *
     * @param labelFilter the label filter.
     * 
     * @note Labels are by default filter to scope, this api is to allow filtering by possible actions.
     * @note If not set HyokProtection and DoubleKeyProtection are filtered.
     */
#if !defined(SWIG) && !defined(SWIG_DIRECTORS)
    [[deprecated("SetLabelFilter is deprecated, use ConfigureFunctionality")]]
#endif
    void SetLabelFilter(const std::vector<LabelFilterType>& deprecatedLabelFilters) {
      mDeprecatedLabelFilters = deprecatedLabelFilters;
    }
    /**
     * @brief Gets the label filters set through deprecated function SetLabelFilter
     *
     * @return the label filter.
     * 
     * @note Labels are by default filter to scope, this api is to allow filtering by possible actions.
     */
    const std::vector<LabelFilterType>& GetLabelFilter() const { return mDeprecatedLabelFilters; }

    /**
     * @brief Enables or disables functionality
     *
     * @param functionalityFilterType the type of functionality.
     * @param enabled True to enable, false to disable
     * 
     * @note HyokProtection, DoubleKeyProtection, DoubleKeyUserDefinedProtection are disabled by default and must be enabled
     */
    void ConfigureFunctionality(FunctionalityFilterType functionalityFilterType, bool enabled) {
      if(functionalityFilterType == FunctionalityFilterType::None) {
        throw BadInputError(
            "FunctionalityFilterType::None is not supported");
      }

      mConfiguredFunctionality[functionalityFilterType] = enabled;
    }

    /**
     * @brief Gets the configured functionality
     *
     * @return A map of the types to a boolean value indicating whether or not it is enabled
     */
    const std::map<FunctionalityFilterType, bool>& GetConfiguredFunctionality() const { return mConfiguredFunctionality; }

    /**
     * @brief Sets the variable text marking type
     *
     * @param variableTextMarkingType the variable text marking type.
     *
     */
    void SetVariableTextMarkingType(VariableTextMarkingType variableTextMarkingType) {
      mVariableTextMarkingType = variableTextMarkingType;
    }

    /**
     * @brief Gets the variable text marking type
     *
     * @return the variable text marking type.
     *
     */
    VariableTextMarkingType GetVariableTextMarkingType() const {
      return mVariableTextMarkingType;
    }

    /**
     * @brief Set the Engine Auth Delegate.
     * 
     * @param authDelegate the Auth delegate
     */
    void SetAuthDelegate(const std::shared_ptr<AuthDelegate>& authDelegate) { 
      mAuthDelegate = authDelegate; 
    }

    /**
     * @brief Get the Engine Auth Delegate.
     * 
     * @return the Engine Auth Delegate. 
     */
    std::shared_ptr<AuthDelegate> GetAuthDelegate() const { return mAuthDelegate; }

#if !defined(SWIG) && !defined(SWIG_DIRECTORS)
    /**
     * @brief Get logger context that will be opaquely passed to the logger delegate for logs associated with the created engine
     * 
     * @return The logger context
     */
    const std::shared_ptr<void>& GetLoggerContext() const { return mLoggerContext; }
#endif
    /**
     * @brief Sets the logger context that will be opaquely passed to the logger delegate for logs associated with the created engine
     * 
     * @param loggerContext The logger context
     * 
     */
    void SetLoggerContext(const std::shared_ptr<void>& loggerContext) {
      mLoggerContext = loggerContext;
    }

    /** @cond DOXYGEN_HIDE */
    ~Settings() {}
  private:
    std::string mEngineId;
    Identity mIdentity;
    Cloud mCloud = Cloud::Unknown;
    DataBoundary mDataBoundary = DataBoundary::Default;
    std::shared_ptr<AuthDelegate> mAuthDelegate;
    std::string mClientData;
    std::vector<std::pair<std::string, std::string>> mCustomSettings;
    std::vector<LabelFilterType> mDeprecatedLabelFilters;      // Labels that the client does not want to view
    std::map<FunctionalityFilterType, bool> mConfiguredFunctionality;  // Functionality that has been turned on or off
    std::string mLocale;
    std::string mSessionId;
    bool mIsLoadSensitivityTypesEnabled;
    std::string mCloudEndpointBaseUrl;
    std::string mDelegatedUserEmail;
    VariableTextMarkingType mVariableTextMarkingType = VariableTextMarkingType::Default;
    std::map<Classifier, bool> mClassifierSupport;  // Overwritten classifiers that the application elects to support or not
    std::shared_ptr<void> mLoggerContext;
    /** @endcond */
  };

  /**
   * @brief Get the policy engine Settings. 
   * 
   * @return policy engine settings.
   * @see mip::PolicyEngine::Settings
   */
  virtual const Settings& GetSettings() const = 0;

  /**
   * @brief list the sensitivity labels associated with the policy engine according to the provided contentFormats.
   * 
   * @param contentFormats contentFormats Vector of formats to filter the sensitivity labels by, such as "file", "email", etc.
   *  Set contentFormats to an empty vector to filter the sensitivity labels by the default formats.
   * 
   * @return a list of sensitivity labels.
   */
  virtual const std::vector<std::shared_ptr<Label>> ListSensitivityLabels(
      const std::vector<std::string>& contentFormats = std::vector<std::string>()) = 0;

  /**
   * @brief list the sensitivity types associated with the policy engine.
   * 
   * @return a list of sensitivity labels. empty if LoadSensitivityTypesEnabled was false (@see PolicyEngine::Settings).
   */
  virtual const std::vector<std::shared_ptr<SensitivityTypesRulePackage>>& ListSensitivityTypes() const = 0;

  /**
   * @brief Provide a url for looking up more information about the policy/labels.
   * 
   * @return a url in string format.
   */
  virtual const std::string& GetMoreInfoUrl() const = 0;

  /**
   * @brief Checks if the policy dictates that a content must be labeled or not according to the provided contentFormat.
   * 
   * @param contentFormat The format to filter by when determining whether a label is required - example: "file", "email", etc.
   *  Set contentFormat to an empty string to determine whether labeling is required for the default format.
   * 
   * @return true if labeling is mandatory, else false. 
   */
  virtual bool IsLabelingRequired(const std::string& contentFormat = std::string()) const = 0; 

  /**
   * @brief Checks if the policy dictates that given a label sensitivity level downgrade requires a justification message.
   * 
   * @return true if downgrade justification is required, else false.
   */
  virtual bool IsDowngradeJustificationRequired() const = 0; 

  /**
   * @brief Get the default sensitivity label according to the provided contentFormat. 
   * 
   * @param contentFormat The format to filter by when retrieving the default sensitivity label - example: "file", "email", etc. 
   * Set contentFormat to an empty string to retrieve the default sensitivity label for the default format.
   * 
   * @return default sensitivity label if exists, nullptr if there is no default label set.
   */
  virtual const std::shared_ptr<Label> GetDefaultSensitivityLabel(const std::string& contentFormat = std::string()) const = 0;

  /**
   * @brief Gets the label according to the provided id.
   * 
   * @param id Identifier for the label.
   * 
   * @return Label
   */
  virtual std::shared_ptr<Label> GetLabelById(const std::string& id) const = 0;

  /**
   * @brief Create a Policy Handler to execute policy-related functions on a file's execution state.
   * 
   * @param isAuditDiscoveryEnabled Describes whether audit discovery is enabled or not.
   * 
   * @return Policy Handler.
   * 
   * @note Application needs to keep the policy handler object for the lifetime of the document.
   */
  virtual std::shared_ptr<PolicyHandler> CreatePolicyHandler(bool isAuditDiscoveryEnabled, bool isGetSensitivityLabelAuditDiscoveryEnabled = true) = 0;

  /**
   * @brief Logs an application specific event to the audit pipeline.
   *
   * @param level of the log level: Info/Error/Warning.
   * @param eventType a description of the type of event.
   * @param eventData the data associated with the event.
   */
  virtual void SendApplicationAuditEvent(
      const std::string& level,
      const std::string& eventType,
      const std::string& eventData) = 0;

  /**
   * @brief Gets tenant ID associated with engine
   * 
   * @return Tenant ID
   */
  virtual const std::string& GetTenantId() const = 0;

  /**
   * @brief Gets policy data XML which describes the settings, labels, and rules associated with this policy.
   * 
   * @return Policy data XML.
   */
  virtual const std::string& GetPolicyDataXml() const = 0;

  /**
   * @brief Gets sensitivity types data XML which describes the sensitivity types associated with this policy.
   * 
   * @return Sensitivity types data XML.
   */
  virtual const std::string& GetSensitivityTypesDataXml() const = 0;

  /**
   * @brief Gets a list of custom settings.
   * 
   * @return a vector of custom settings.
   */
  virtual const std::vector<std::pair<std::string, std::string>>& GetCustomSettings() const = 0;

  /**
   * @brief Gets the policy file ID
   *
   * @return a string that represent the policy file ID
   */
  virtual const std::string& GetPolicyFileId() const = 0;

   /**
   * @brief Gets the sensitivity file ID
   *
   * @return a string that represent the policy file ID
   */
  virtual const std::string& GetSensitivityFileId() const = 0;

  /**
   * @brief Gets if the policy has automatic or recommendation rules according to the provided contentFormats 
   * 
   * @param contentFormat Vector of formats to consider when determining if a rule is defined for any provided format. 
   * Set contentFormats to an empty vector indicates the provided contentFormats are default formats.
   *
   * @return a bool that will tell if there any automatic or recommendation rules in 
   *  the policy
   */
  virtual bool HasClassificationRules(const std::vector<std::string>& contentFormats = std::vector<std::string>()) const = 0;

  /**
   * @brief Gets the time when the policy was last fetched
   *
   * @return the time when the policy was last fetched
   */
  virtual std::chrono::time_point<std::chrono::system_clock> GetLastPolicyFetchTime() const = 0;

  /**
   * @brief Gets the recommended WXP (Word, Excel, Powerpoint) metadata version, currently 0 for old verion
   * 1 for co-authoring enabled version. 
   *
   * @return uint32_t int indecating what version of metadata the tenant supports for WXP files.
   */
  virtual uint32_t GetWxpMetadataVersion() const = 0;

  /**
   * @brief Checks if user has consented to specific workload, 
   *
   * @return bool indicating consent.
   */
  virtual bool HasWorkloadConsent(Workload workload) const = 0;

  /** @cond DOXYGEN_HIDE */
  virtual ~PolicyEngine() { }

protected:
/** @cond DOXYGEN_HIDE */
  PolicyEngine() {}
  /** @endcond */
};

MIP_NAMESPACE_END

#endif  // API_MIP_UPE_POLICY_ENGINE_H_
