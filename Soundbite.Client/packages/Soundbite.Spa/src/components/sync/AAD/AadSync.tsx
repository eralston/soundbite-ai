import { observer } from "mobx-react-lite";
import React from "react";
import {
  AadOrgConfig,
  CollectionSyncMode,
  OrgSyncConfig,
  SyncValidation,
} from "@soundbite/api";
import {
  Loader,
  ShowWhen,
  Toggle,
  WidgetStore,
} from "@soundbite/widgets-react";

import { getTenantId } from "../../../sdk/providers/auth/AadAuthProvider";
import SpaStore from "../../../store/Spa.Store";
import { AadTenantPicker } from "./AadTenantPicker";
import { AadIntro } from "./AadIntro";
import { AadGrant } from "./AadGrant";
import { SyncType } from "../../../modules/Sync";
import { TargetConfig, TargetConfigMode } from "../TargetConfig";
import { FormGroup, InputGroup } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPhone } from "@fortawesome/free-solid-svg-icons";

interface IProps {
  orgRoute: string;
  onChange: (newStrategyConfig?: object) => void;
}

/** React component depicting the AAD Sync-specific sync settings */
export const AadSync: React.FC<IProps> = observer((props: IProps) => {
  const readAadConfig = (json?: string) => {
    if (json == null) return undefined;
    const aadConfig: AadOrgConfig | undefined = JSON.parse(json);
    return aadConfig;
  };

  const nextStrategyConfig =
    WidgetStore.sync.nextStrategyConfig<AadOrgConfig>();

  const baseAadConfig = async (): Promise<AadOrgConfig> => {
    const latestAadConfig =
      WidgetStore.sync.validation?.sanitizedConfig?.syncConfigJson ??
      WidgetStore.sync.config?.sanitizedConfig?.syncConfigJson;
    const valAadConfig = readAadConfig(latestAadConfig);
    const ret: AadOrgConfig = {
      tenantId:
        valAadConfig?.tenantId ??
        nextStrategyConfig?.tenantId ??
        (await getTenantId()),
      appId: valAadConfig?.appId,
      secretKey: valAadConfig?.secretKey,
      syncType: nextStrategyConfig?.syncType ?? SyncType.AAD,
      importPhoneNumbers: nextStrategyConfig?.importPhoneNumbers ?? true,
    };
    return ret;
  };

  // Event handlers
  const onGrant = async () => {
    // Edge case where they want to connect ia AAD, but don't have default providerconfig yet
    const oldConfig = WidgetStore.sync.validation?.sanitizedConfig;
    if (
      oldConfig != null &&
      oldConfig?.syncType === SyncType.AAD &&
      oldConfig?.syncConfigJson == null
    ) {
      // If default provider config allows access, then just connect them
      const reqConfig = { ...oldConfig };
      reqConfig.syncConfigJson = JSON.stringify(baseAadConfig());
      const newVal = await WidgetStore.sync.validateConfigAsync(
        props.orgRoute,
        reqConfig
      );
      if (newVal.hasAccess) {
        onConnect();
        return;
      }
    }

    await SpaStore.grantAdminAsync();
  };

  const onChangeTenantId = (tenantId?: string) => {
    const current = WidgetStore.sync.nextStrategyConfig<AadOrgConfig>();
    const nextStrategyConfig = {
      ...current,
      tenantId,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onChangeGroups = (newConfig: AadOrgConfig) => {
    const current = WidgetStore.sync.nextStrategyConfig<AadOrgConfig>();
    const nextStrategyConfig = {
      ...current,
      groups: newConfig.groups,
      groupsMode: newConfig.groupsMode,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onChangeUsers = (newConfig: AadOrgConfig): void => {
    const currentConfig = WidgetStore.sync.nextStrategyConfig<AadOrgConfig>();
    const nextStrategyConfig = {
      ...currentConfig,
      users: newConfig.users,
      usersMode: newConfig.usersMode,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onConnect = async (): Promise<void> => {
    const tenantId = nextStrategyConfig?.tenantId ?? (await getTenantId());
    const nextConfig: AadOrgConfig = {
      tenantId,
      groupsMode: CollectionSyncMode.None,
      usersMode: CollectionSyncMode.None,
      syncType: nextStrategyConfig?.syncType ?? SyncType.AAD,
      ...nextStrategyConfig,
    };
    const newOrgConfig: OrgSyncConfig = {
      syncConfigJson: JSON.stringify(nextConfig),
      syncType: SyncType.AAD,
    };
    await WidgetStore.sync.updateConfigAsync(props.orgRoute, newOrgConfig);
  };

  const isReady = (config?: SyncValidation): boolean => {
    return (
      config != null &&
      config?.hasAccess &&
      config.sanitizedConfig?.syncType === SyncType.AAD &&
      config.validationMessage == null
    );
  };

  const onTogglePhoneNumbers = () => {
    const nextState = !(nextStrategyConfig?.importPhoneNumbers ?? true);

    const currentConfig = WidgetStore.sync.nextStrategyConfig<AadOrgConfig>();
    const nextConfig = {
      ...currentConfig,
      importPhoneNumbers: nextState,
    };
    WidgetStore.sync.setNextStrategyConfig(nextConfig);
    props.onChange(nextConfig);
  };

  const isSavedReady = isReady(WidgetStore.sync.config);

  const AadConfigView: React.FC = () => {
    const isReadyNow =
      isSavedReady &&
      WidgetStore.sync.config != null &&
      nextStrategyConfig != null;

    let msg = "Reading Settings...";

    return (
      <Loader isLoadedWhen={isReadyNow} message={msg}>
        <TargetConfig
          defaultConfig={nextStrategyConfig}
          onChange={onChangeGroups}
          orgRoute={props.orgRoute}
          mode={TargetConfigMode.Groups}
          syncType={SyncType.AAD}
        >
          <h3 className="mb-0">
            Groups
            <small className="text-muted ml-2">
              Microsoft 365 and AD Security Groups, plus Exchange Distribution
              Lists (Non-Nested) and Mail-Enabled Groups.{" "}
              <a
                rel="noreferrer"
                target="_blank"
                href="https://portal.azure.com/#blade/Microsoft_AAD_IAM/GroupsManagementMenuBlade/AllGroups"
              >
                View AAD Groups
              </a>
            </small>
          </h3>
        </TargetConfig>
        <TargetConfig
          defaultConfig={nextStrategyConfig}
          onChange={onChangeUsers}
          orgRoute={props.orgRoute}
          mode={TargetConfigMode.Users}
          syncType={SyncType.AAD}
        >
          <h3 className="mb-0">
            Users
            <small className="text-muted ml-2">
              AD Users including guests; potentially licensed or unlicensed for
              M365{" "}
              <a
                rel="noreferrer"
                target="_blank"
                href="https://portal.azure.com/#blade/Microsoft_AAD_IAM/UsersManagementMenuBlade/MsGraphUsers"
              >
                View AAD Users
              </a>
            </small>
          </h3>
        </TargetConfig>
        <FormGroup className="sb-datepicker">
          <InputGroup>
            <div className="input-group-prepend">
              <div className="input-group-text">
                <FontAwesomeIcon icon={faPhone} />
              </div>
            </div>
            <Toggle
              defaultValue={nextStrategyConfig?.importPhoneNumbers ?? false}
              onToggle={onTogglePhoneNumbers}
              title="Import Phone Number"
            />
            <label className="sb-datepicker-none-label text-muted">
              Import Phone Numbers for SMS Alerts
            </label>
          </InputGroup>
        </FormGroup>
      </Loader>
    );
  };

  const latestConfig = WidgetStore.sync.validation ?? WidgetStore.sync.config;
  const isWaitingOnValidation =
    WidgetStore.sync.config?.sanitizedConfig?.syncType !== SyncType.AAD &&
    WidgetStore.sync.validation == null;

  return (
    <div className="sb-sync-form-aad">
      <Loader
        isLoadedWhen={latestConfig != null && !isWaitingOnValidation}
        message="Validating Active Directory Configuration..."
        style={{ minHeight: "12rem" }}
      >
        <AadTenantPicker
          defaultTenantId={nextStrategyConfig?.tenantId}
          onChange={(newTenantId?: string) => {
            onChangeTenantId(newTenantId);
          }}
        />
        <ShowWhen is={!latestConfig?.hasAccess}>
          <AadGrant onGrant={onGrant} />
        </ShowWhen>
        <ShowWhen is={latestConfig?.hasAccess}>
          <ShowWhen is={isSavedReady}>
            <AadConfigView />
          </ShowWhen>
          <ShowWhen is={!isSavedReady}>
            <AadIntro />
          </ShowWhen>
        </ShowWhen>
      </Loader>
    </div>
  );
});
