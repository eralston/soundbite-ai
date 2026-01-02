import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  Alert,
  Button,
  Card,
  CardBody,
  CardFooter,
  CardHeader,
  Col,
  Row,
} from "reactstrap";

import { OrganizationsService, OrgAzureSettings } from "@soundbite/api";
import { ErrorDlg, Loader, WidgetStore } from "@soundbite/widgets-react";
import { AadTenantPicker } from "../sync/AAD/AadTenantPicker";

import { ToggleRow, ToggleTable } from "../controls/ToggleTable";

interface TeamsNotificationsCardProps {
  className?: string;
}

/** Configuration card for MS Teams Notifications */
export const TeamsNotificationsCard: React.FC<TeamsNotificationsCardProps> =
  observer((props: TeamsNotificationsCardProps) => {
    // Params
    let { orgRoute } = useParams<{ orgRoute: string }>();

    const [settings, setSettings] = useState<OrgAzureSettings | undefined>(
      undefined
    );
    const [isChanged, setIsChanged] = useState(false);
    const [isSaving, setIsSaving] = useState(false);

    useEffect(() => {
      const load = async () => {
        const initialSettings =
          await OrganizationsService.readAzureSettingsAsync(orgRoute);
        setSettings(initialSettings);
        setIsChanged(false);
      };
      load();
    }, [orgRoute]);

    const onSave = async () => {
      // Must have settings; otherwise, just ignore
      if (settings == null) {
        return;
      }

      setIsSaving(true);
      try {
        await OrganizationsService.updateAzureSettingsAsync(orgRoute, settings);
        setIsChanged(false);
      } catch (err) {
        ErrorDlg.show(err, "Error Saving Notification Settings");
      }
      setIsSaving(false);
    };

    return (
      <Card className={props.className}>
        <Loader
          isLoadedWhen={!!WidgetStore.organizations.currentOrg && !!settings}
          style={{ minHeight: "12rem" }}
        >
          <CardHeader className="border-0">
            <Row className="align-items-center">
              <Col md="11">
                <h1 className="mb-0">
                  <i className="fas fa-bell d-none d-sm-inline"></i>
                  {WidgetStore.organizations.currentOrg?.details.name} Teams
                  Notification Settings
                </h1>
                <div className="mt-0 text-muted">
                  Configure notifications for Microsoft Teams
                </div>
              </Col>
              <Col md="1" className="text-right"></Col>
            </Row>
          </CardHeader>
          <CardBody>
            <Alert color="light">
              Microsoft Teams integration requires installing the latest version
              of the{" "}
              <a
                href="https://github.com/Soundbite-ai/api/tree/main/releases/teamsApp"
                target="_blank"
                rel="noreferrer"
              >
                Soundbite for Microsoft Teams app
              </a>{" "}
              For more info please contact{" "}
              <a
                className="alert-link"
                href="mailto:Support@Soundbite.Freshdesk.Com"
              >
                Soundbite Support
              </a>
            </Alert>
            <AadTenantPicker
              defaultTenantId={settings?.tenantId}
              onChange={(newTenantId?: string) => {
                setSettings({
                  ...settings,
                  enableTeamsNotifications:
                    settings?.enableTeamsNotifications ?? false,
                  tenantId: newTenantId,
                });
                setIsChanged(true);
              }}
            />
          </CardBody>
          <div className="table-responsive">
            <ToggleTable hideHeader={true}>
              <ToggleRow
                title="Teams Notifications"
                subtitle="Enable or disable MS Teams activity - must install Teams app"
                onToggle={(enabled: boolean) => {
                  settings != null &&
                    setSettings({
                      ...settings,
                      enableTeamsNotifications: enabled,
                    });
                  setIsChanged(true);
                }}
                defaultValue={settings?.enableTeamsNotifications ?? false}
              />
            </ToggleTable>
          </div>
          <CardFooter className="d-flex justify-content-end">
            <Button
              color="primary"
              className="btn-icon btn-3"
              onClick={onSave}
              disabled={!isChanged && !isSaving}
              title="Save Teams Notification Settings"
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
    );
  });
