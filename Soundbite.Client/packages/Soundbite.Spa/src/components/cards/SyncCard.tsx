import {
  AadOrgConfig,
  OktaOrgSyncConfig,
  OrgSyncConfig,
  OrgSyncState,
  SyncStrategyConfigBase,
  SyncValidation,
} from "@soundbite/api";
import {
  ErrorDlg,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";
import classnames from "classnames";
import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  Alert,
  Button,
  Card,
  CardBody,
  CardFooter,
  CardHeader,
  Nav,
  NavItem,
  NavLink,
  TabContent,
  TabPane,
} from "reactstrap";
import { AadSync } from "../sync/AAD/AadSync";
import { SyncDisabled } from "../sync/SyncDisabled";
import { doConfigsMatch, SyncType } from "../../modules/Sync";
import { getTenantId } from "../../sdk/providers/auth/AadAuthProvider";
import { OktaSync } from "../sync/Okta/OktaSync";
import { SyncHistory } from "../sync/SyncHistory";

export const SyncCard: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  // State
  const [isDirty, setIsDirty] = useState(false);
  const [isSyncing, setIsSyncing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isValidating, setIsValidating] = useState(false);
  const [syncMode, setSyncMode] = useState(SyncType.Disabled);

  // Render
  const latestConfig = WidgetStore.sync.validation ?? WidgetStore.sync.config;

  const date = new Date();
  date.setUTCHours(0, 0, 0, 0);
  date.setDate(date.getDate() + 1);
  const nextRunDate = date.toLocaleString();

  const tabValueForConfig = (config?: OrgSyncConfig) => {
    if (config?.syncType === SyncType.AAD) return SyncType.AAD;
    if (config?.syncType === SyncType.Okta) return SyncType.Okta;
    return SyncType.Disabled;
  };

  const validate = async (
    orgConfig?: OrgSyncConfig
  ): Promise<SyncValidation> => {
    setIsValidating(true);
    const validation = await WidgetStore.sync.validateNextConfig(orgRoute);
    setIsValidating(false);
    checkDirty();
    return validation;
  };

  const setFromConfig = (savedConfig: SyncValidation): void => {
    setSyncMode(tabValueForConfig(savedConfig.sanitizedConfig));
    setIsDirty(false);
  };

  const load = async () => {
    try {
      if (orgRoute == null)
        throw new Error(
          "Cannot load sync settings without first connecting to an organization"
        );
      // Start
      setIsLoading(true);
      WidgetStore.sync.reset();
      // Load
      const newOrgConfig = await WidgetStore.sync.readConfigAsync(orgRoute);
      setFromConfig(newOrgConfig);
      // End
      setIsLoading(false);
    } catch (err: any) {
      WidgetStore.sync.reset();
      ErrorDlg.show(err, "Cannot load organization sync config");
    }
  };

  const save = async () => {
    // Start
    setIsSaving(true);
    try {
      // Save
      const newVal = await WidgetStore.sync.updateToNextConfig(orgRoute);
      setFromConfig(newVal);
    } catch (err) {
      ErrorDlg.show(err);
    }

    // End
    setIsSaving(false);
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line
  }, []); // Fire once

  const onChangeStrategy = (newStrategyConfig?: object): void => {
    checkDirty();
  };

  // TODO: Hide this detail in an abstraction
  function readConfig<TConfig>(json?: string) {
    if (json == null) return undefined;
    const config: TConfig | undefined = JSON.parse(json);
    return config;
  }

  const initAad = async (): Promise<AadOrgConfig> => {
    const latestJson =
      WidgetStore.sync.validation?.sanitizedConfig?.syncConfigJson ??
      WidgetStore.sync.config?.sanitizedConfig?.syncConfigJson;
    const latestAadConfig = readConfig<AadOrgConfig>(latestJson);
    const ret: AadOrgConfig = {
      tenantId: latestAadConfig?.tenantId ?? (await getTenantId()),
      appId: latestAadConfig?.appId,
      secretKey: latestAadConfig?.secretKey,
      syncType: latestAadConfig?.syncType ?? SyncType.AAD,
    };
    return ret;
  };

  const initOkta = async (): Promise<OktaOrgSyncConfig> => {
    const latestJson =
      WidgetStore.sync.validation?.sanitizedConfig?.syncConfigJson ??
      WidgetStore.sync.config?.sanitizedConfig?.syncConfigJson;
    const latestConfig = readConfig<OktaOrgSyncConfig>(latestJson);
    const ret: OktaOrgSyncConfig = {
      domain: latestConfig?.domain ?? "",
      apiToken: latestConfig?.apiToken ?? "",
      syncType: latestConfig?.syncType ?? SyncType.Okta,
      isPartialSync: latestConfig?.isPartialSync ?? false,
      maxRunLength: latestConfig?.maxRunLength ?? 1000 * 60 * 8 /* 8 Minutes */,
      userSyncComplete: latestConfig?.userSyncComplete ?? false,
    };
    return ret;
  };

  const init = async () => {
    // TODO: Hide this detail in an abstraction
    let nextStrategy =
      WidgetStore.sync.nextStrategyConfig<SyncStrategyConfigBase>();
    if (nextStrategy == null) {
      if (syncMode === SyncType.AAD) {
        nextStrategy = await initAad();
      } else if (syncMode === SyncType.Okta) {
        nextStrategy = await initOkta();
      }
    }
    const nextStrategyJson =
      nextStrategy == null ? undefined : JSON.stringify(nextStrategy);

    const nextConfig: OrgSyncConfig = {
      syncConfigJson: nextStrategyJson,
      ...WidgetStore.sync.nextConfig,
      syncType: syncMode === SyncType.Disabled ? undefined : syncMode,
    };
    WidgetStore.sync.setNextConfig(nextConfig);

    validate();
  };

  // When the sync mode OR provider config changes, then validate
  useEffect(
    () => {
      init();
    },
    // eslint-disable-next-line
    [syncMode]
  );

  const switchMode = (newSyncMode: SyncType) => {
    if (newSyncMode === syncMode) return;
    setSyncMode(newSyncMode);
  };

  const checkDirty = (): void => {
    // Pull out the target configs
    const nextConfig = WidgetStore.sync.nextConfig;
    let lastConfig = WidgetStore.sync.config?.sanitizedConfig;

    if (
      WidgetStore.sync.validation != null &&
      lastConfig?.syncType !==
        WidgetStore.sync.validation?.sanitizedConfig?.syncType
    ) {
      lastConfig = WidgetStore.sync.validation?.sanitizedConfig;
    }

    // Set dirty state
    const dirty = !doConfigsMatch(nextConfig, lastConfig);
    setIsDirty(dirty);
  };

  const onSave = async () => {
    try {
      if (orgRoute == null) {
        throw new Error(
          "Cannot save sync settings without loading the configuration first"
        );
      }

      // If we have nothing to change, then do nothing
      if (!isDirty) return;

      await save();
    } catch (err: any) {
      ErrorDlg.show(err);
    }
  };

  // Check for dirty when config components change
  useEffect(() => {
    checkDirty();
    // eslint-disable-next-line
  }, [syncMode]);

  const sync = async () => {
    setIsSyncing(true);
    await WidgetStore.sync.syncAsync(orgRoute);
    setIsSyncing(false);
  };

  const createTab = (title: string, value: SyncType) => {
    const className = classnames({ active: value === syncMode }, "sb-tab-btn");

    return (
      <NavItem>
        <NavLink
          role="tab"
          aria-selected={syncMode === value}
          className={className}
          onClick={() => {
            switchMode(value);
          }}
        >
          {title}
        </NavLink>
      </NavItem>
    );
  };

  const saveBtn = (isShow: boolean) => {
    const animationClass = isSaving ? "fa-spin" : "";
    const iconClass = isSaving ? "fa-cog" : "fa-save";
    return (
      <ShowWhen is={isShow}>
        <Button
          color="primary"
          className="btn-icon btn-3"
          onClick={onSave}
          disabled={isSyncing || isSaving}
          title="Save Sync Settings"
        >
          <span className="btn-inner--icon">
            <i className={`fas ${iconClass} ${animationClass}`}></i>
          </span>
          <span className="btn-inner--text">
            <Fragment>&nbsp;</Fragment>Save
          </span>
        </Button>
      </ShowWhen>
    );
  };

  const syncBtn = (isShow: boolean) => {
    const syncMessage = isDirty ? "Save & Sync" : "Sync";
    const animationClass = isSyncing ? "fa-spin" : "";
    const syncState = WidgetStore.sync.result?.state;
    const isShowPending =
      syncState === OrgSyncState.Pending ||
      syncState === OrgSyncState.Processing;
    return (
      <ShowWhen is={isShow}>
        <ShowWhen is={isShowPending}>
          <span className="mr-2 text-muted">
            Sync successfully pending; Refresh history below to see more
          </span>
        </ShowWhen>
        <Button
          color="secondary"
          className="btn-icon btn-3"
          onClick={sync}
          disabled={isSyncing || isSaving}
          title="Sync Organization Now"
        >
          <span className="btn-inner--icon">
            <i className={`fas fa-sync ${animationClass}`}></i>
          </span>
          <span className="btn-inner--text">
            <Fragment>&nbsp;</Fragment>
            {syncMessage}
          </span>
        </Button>
      </ShowWhen>
    );
  };

  const Footer: React.FC = () => {
    const savedConfig = WidgetStore.sync.config;
    const isSyncableModeSaved =
      !!savedConfig?.hasAccess && savedConfig?.validationMessage == null;

    const isShowSync =
      isSyncableModeSaved && !isLoading && !isDirty && !isValidating;

    const isWaitingOnAccess =
      syncMode !== SyncType.Disabled && !latestConfig?.hasAccess;

    const isShowSave =
      !isLoading && isDirty && !isValidating && !isWaitingOnAccess;

    return (
      <ShowWhen is={isShowSave || isShowSync}>
        <CardFooter className="d-flex justify-content-end">
          {saveBtn(isShowSave)}
          {syncBtn(isShowSync)}
        </CardFooter>
        <CardFooter>
          <SyncHistory orgRoute={orgRoute} />
        </CardFooter>
      </ShowWhen>
    );
  };

  const AadView: React.FC = () => {
    const isLoaded = WidgetStore.sync.config != null && !isLoading && !isSaving;
    return (
      <Loader isLoadedWhen={isLoaded}>
        <AadSync orgRoute={orgRoute} onChange={onChangeStrategy} />
      </Loader>
    );
  };

  const OktaView: React.FC = () => {
    const isLoaded = WidgetStore.sync.config != null && !isLoading && !isSaving;
    return (
      <Loader isLoadedWhen={isLoaded}>
        <OktaSync orgRoute={orgRoute} onChange={onChangeStrategy} />
      </Loader>
    );
  };

  const Body: React.FC = () => {
    let loadingMessage = "Loading Configuration...";
    if (isValidating) loadingMessage = "Validating Configuration...";

    return (
      <CardBody className="mt-0 pt-0">
        <Loader
          isLoadedWhen={!isLoading && !isValidating}
          message={loadingMessage}
        >
          <ShowWhen is={latestConfig?.validationMessage != null}>
            <Alert color="warning">
              Soundbite will not be able to sync:{" "}
              {latestConfig?.validationMessage}
            </Alert>
          </ShowWhen>
          <div className="sb-settings-view">
            <div className="d-flex flex-md-row flex-row">
              <Nav className="sb-3-tab flex-column flex-md-row" role="tablist">
                {createTab("Disabled", SyncType.Disabled)}
                {createTab("Azure Active Directory (M365)", SyncType.AAD)}
                {createTab("Okta", SyncType.Okta)}
              </Nav>
            </div>
            <TabContent activeTab={syncMode} className="sb-p-2-tab">
              <TabPane tabId={SyncType.Disabled}>
                <SyncDisabled />
              </TabPane>
              <TabPane tabId={SyncType.AAD}>
                <AadView />
              </TabPane>
              <TabPane tabId={SyncType.Okta}>
                <OktaView />
              </TabPane>
            </TabContent>
          </div>
        </Loader>
      </CardBody>
    );
  };

  return (
    <Card>
      <Loader
        isLoadedWhen={!!WidgetStore.organizations.currentOrg}
        style={{ minHeight: "12rem" }}
      >
        <CardHeader className="border-0">
          <h1 className="mb-0">
            <i className="fas fa-user-plus d-none d-sm-inline"></i>
            {WidgetStore.organizations.currentOrg?.details.name} Directory Sync
          </h1>
          <div className="mt-0 text-muted">
            Next automatic run is <strong>{nextRunDate}</strong> local time.
          </div>
        </CardHeader>
        <Body />
        <Footer />
      </Loader>
    </Card>
  );
});
