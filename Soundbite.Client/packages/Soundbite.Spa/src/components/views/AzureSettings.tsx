import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardFooter, CardHeader, Col, Row } from "reactstrap";

import { OrganizationsService, OrgAzureSettings } from "@soundbite/api";
import {
  ErrorDlg,
  Loader,
  OrganizationStore,
  WidgetStore,
} from "@soundbite/widgets-react";

import { Private } from "./Private";

import {
  TextRow,
  ToggleHeader,
  ToggleRow,
  ToggleTable,
} from "../controls/ToggleTable";

export const AzureSettings: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  const [settings, setSettings] = useState<OrgAzureSettings | undefined>(
    undefined
  );
  const [isChanged, setIsChanged] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    const load = async () => {
      const initialSettings = await OrganizationsService.readAzureSettingsAsync(
        orgRoute
      );
      setSettings(initialSettings);
      setIsChanged(false);
    };
    load();
  }, [orgRoute]);

  const onSave = async () => {
    // Must have settings; otherwise, just ignore
    if (settings != null) {
      setIsSaving(true);
      try {
        let updatedSettings =
          await OrganizationsService.updateAzureSettingsAsync(
            orgRoute,
            settings
          );
        setSettings(updatedSettings);
        // Update the settings on the current org so they take effect immediately
        if (OrganizationStore.currentOrg) {
          OrganizationStore.currentOrg.settings.azure = updatedSettings;
        }
        setIsChanged(false);
      } catch (err) {
        ErrorDlg.show(err, "Error Saving Azure Settings");
      }
      setIsSaving(false);
    }
  };

  return (
    <Private>
      <Card>
        <Loader
          isLoadedWhen={!!WidgetStore.organizations.currentOrg && !!settings}
          style={{ minHeight: "12rem" }}
        >
          <CardHeader className="border-0">
            <Row className="align-items-center">
              <Col md="11">
                <h1 className="mb-0">
                  <i className="fas fa-bell d-none d-sm-inline"></i>
                  {WidgetStore.organizations.currentOrg?.details.name} Azure
                  Settings
                </h1>
                <div className="mt-0 text-muted">
                  Configure the Azure settings for your organization
                </div>
              </Col>
              <Col md="1" className="text-right"></Col>
            </Row>
          </CardHeader>
          <div className="table-responsive">
            <ToggleTable title="Tenant">
              <TextRow
                title="Tenant ID"
                subtitle="Azure Tenant ID associated with your oranization"
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({ ...settings, tenantId: value });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.tenantId ?? ""}
              />
              <ToggleHeader title={"Microsoft Teams"} />
              <ToggleRow
                title="Enable Teams Notifications"
                subtitle="Specifies whether notifications can be sent via Microsoft Teams"
                onToggle={(enabled: boolean) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      enableTeamsNotifications: enabled,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.enableTeamsNotifications ?? true}
              />
              <ToggleHeader title={"Media Services"} />
              <TextRow
                title="Media Services Account Name"
                subtitle="Media Services account named associated with the organization."
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      mediaServiceAccountName: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.mediaServiceAccountName ?? ""}
              />
              <TextRow
                title="Resource Group Name"
                subtitle="Media Services resource group name associated with the media service account."
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      mediaServiceResourceGroupName: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.mediaServiceResourceGroupName ?? ""}
              />
              <TextRow
                title="Single Bitrate Transform Name"
                subtitle="Name of the transform used for encoding single bit rate videos."
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      singleBitRateTransformName: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.singleBitRateTransformName ?? ""}
              />
              <TextRow
                title="Streaming Endpoint URL"
                subtitle="URL of the streaming endpoint associated with the organization"
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      streamingEndpointUrlPrefix: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.streamingEndpointUrlPrefix ?? ""}
              />
              <ToggleHeader title={"Communication Services"} />
              <TextRow
                title="ACS Connection String"
                subtitle="ACS Connection string used to send notification emails"
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      acsConnectionString: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.acsConnectionString ?? ""}
              />
              <TextRow
                title="From Email"
                subtitle="Email address that appears to send notifications"
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      acsFromEmail: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.acsFromEmail ?? ""}
              />
              <TextRow
                title="From Name"
                subtitle="Name that appears to send notifications"
                onChange={(value) => {
                  if (settings != null) {
                    setSettings({
                      ...settings,
                      acsFromName: value,
                    });
                    setIsChanged(true);
                  }
                }}
                defaultValue={settings?.acsFromName ?? ""}
              />
            </ToggleTable>
          </div>
          <CardFooter className="d-flex justify-content-end">
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onSave}
              disabled={!isChanged && !isSaving}
              title="Save Notification Settings"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-save"></i>
              </span>
              <span className="btn-inner--text">
                <Fragment>&nbsp;</Fragment>Save
              </span>
            </Button>
          </CardFooter>
        </Loader>
      </Card>
    </Private>
  );
});
