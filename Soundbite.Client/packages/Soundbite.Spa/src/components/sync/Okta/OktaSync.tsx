import React from "react";
import { observer } from "mobx-react-lite";
import { Button, FormGroup, InputGroup } from "reactstrap";

import {
  CollectionSyncMode,
  OktaOrgSyncConfig,
  OrgSyncConfig,
  SyncStrategyConfigBase,
  SyncValidation,
} from "@soundbite/api";

import {
  FaPrepend,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";

import { SyncType } from "../../../modules/Sync";
import { OktaIntro } from "./OktaIntro";
import { TargetConfig, TargetConfigMode } from "../TargetConfig";

interface IProps {
  orgRoute: string;
  onChange: (newStrategyConfig?: object) => void;
}

/** React component depicting the AAD Sync-specific sync settings */
export const OktaSync: React.FC<IProps> = observer((props: IProps) => {
  const isReady = (config?: SyncValidation): boolean => {
    return (
      config != null &&
      config?.hasAccess &&
      config.sanitizedConfig?.syncType === SyncType.Okta &&
      config.validationMessage == null
    );
  };

  const isSavedReady = isReady(WidgetStore.sync.config);

  const nextStrategyConfig =
    WidgetStore.sync.nextStrategyConfig<OktaOrgSyncConfig>();
  const latestConfig = WidgetStore.sync.validation ?? WidgetStore.sync.config;
  const isWaitingOnValidation =
    WidgetStore.sync.config?.sanitizedConfig?.syncType !== SyncType.Okta &&
    WidgetStore.sync.validation == null;
  const hasAccess =
    WidgetStore.sync.config?.sanitizedConfig?.syncType === SyncType.Okta &&
    latestConfig?.hasAccess;

  const onChangeApiToken = (apiToken?: string) => {
    const current = WidgetStore.sync.nextStrategyConfig<OktaOrgSyncConfig>();
    const nextStrategyConfig = {
      ...current,
      apiToken,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onChangeDomain = (domain?: string) => {
    const current = WidgetStore.sync.nextStrategyConfig<OktaOrgSyncConfig>();
    const nextStrategyConfig = {
      ...current,
      domain,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onConnect = async (): Promise<void> => {
    const nextConfig: OktaOrgSyncConfig = {
      apiToken: nextStrategyConfig?.apiToken ?? "",
      domain: nextStrategyConfig?.domain ?? "",
      groupsMode: CollectionSyncMode.None,
      usersMode: CollectionSyncMode.None,
      syncType: SyncType.Okta,
      isPartialSync: false,
      maxRunLength: 1000 * 60 * 8 /* 8 Minutes */,
      userSyncComplete: false,
    };
    const newOrgConfig: OrgSyncConfig = {
      syncConfigJson: JSON.stringify(nextConfig),
      syncType: SyncType.Okta,
    };
    await WidgetStore.sync.updateConfigAsync(props.orgRoute, newOrgConfig);
  };

  const onChangeGroups = (newConfig: SyncStrategyConfigBase) => {
    const current = WidgetStore.sync.nextStrategyConfig<OktaOrgSyncConfig>();
    const nextStrategyConfig = {
      ...current,
      groups: newConfig.groups,
      groupsMode: newConfig.groupsMode,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const onChangeUsers = (newConfig: SyncStrategyConfigBase): void => {
    const currentConfig =
      WidgetStore.sync.nextStrategyConfig<OktaOrgSyncConfig>();
    const nextStrategyConfig = {
      ...currentConfig,
      users: newConfig.users,
      usersMode: newConfig.usersMode,
    };
    WidgetStore.sync.setNextStrategyConfig(nextStrategyConfig);
    props.onChange(nextStrategyConfig);
  };

  const ConfigView: React.FC = () => {
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
            <small className="text-muted ml-2">Collections of users</small>
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
              Individuals able to sign-in to your Okta tenant
            </small>
          </h3>
        </TargetConfig>
      </Loader>
    );
  };

  const connectionConfig = () => {
    const domainIsEmpty =
      nextStrategyConfig?.domain == null ||
      nextStrategyConfig?.domain.trim() === "";
    const apiTokenIsEmpty =
      nextStrategyConfig?.apiToken == null ||
      nextStrategyConfig?.apiToken.trim() === "";
    const connectDisabled = (domainIsEmpty || apiTokenIsEmpty) && !hasAccess;

    return (
      <>
        <FormGroup>
          <InputGroup>
            <FaPrepend icon="house-user" prefix="fa" />
            <input
              type="text"
              className="form-control"
              placeholder="Domain"
              aria-label="Domain"
              aria-describedby="basic-addon1"
              defaultValue={nextStrategyConfig?.domain}
              onChange={(e) => onChangeDomain(e.target.value)}
              title="Domain"
            />
          </InputGroup>
        </FormGroup>
        <FormGroup>
          <InputGroup>
            <FaPrepend icon="key" prefix="fa" />
            <input
              type="text"
              className="form-control"
              placeholder={hasAccess ? "New API Token" : "API Token"}
              aria-label="API Token"
              aria-describedby="basic-addon1"
              defaultValue={nextStrategyConfig?.apiToken}
              onChange={(e) => onChangeApiToken(e.target.value)}
              title="API Token"
            />
          </InputGroup>
        </FormGroup>
        <ShowWhen is={!hasAccess}>
          <ShowWhen is={connectDisabled}>
            <p className="text-muted">
              Must provide Domain and API Token to Connect
            </p>
          </ShowWhen>
          <Button
            color="primary"
            className="btn-icon btn-3"
            disabled={connectDisabled}
            onClick={onConnect}
          >
            <span className="btn-inner--icon">
              <i className="fas fa-unlock"></i>&nbsp;
            </span>
            <span className="btn-inner--text">Connnect</span>
          </Button>
        </ShowWhen>
        <ShowWhen is={hasAccess}>
          <ConfigView />
        </ShowWhen>
      </>
    );
  };

  return (
    <div className="sb-sync-form-aad">
      <Loader
        isLoadedWhen={latestConfig != null && !isWaitingOnValidation}
        message="Validating Okta Configuration..."
        style={{ minHeight: "12rem" }}
      >
        <ShowWhen is={!hasAccess}>
          <OktaIntro />
        </ShowWhen>
        {connectionConfig()}
      </Loader>
    </div>
  );
});
