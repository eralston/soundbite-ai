import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardFooter, CardHeader, Col, Row } from "reactstrap";

import { OrganizationsService, OrgNotificationSettings } from "@soundbite/api";
import { ErrorDlg, Loader, WidgetStore } from "@soundbite/widgets-react";

import { ToggleRow, ToggleTable } from "../controls/ToggleTable";

/** Configuration card for notifications */
export const NotificationsCard: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  const [settings, setSettings] = useState<OrgNotificationSettings | undefined>(
    undefined
  );
  const [isChanged, setIsChanged] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    const load = async () => {
      const initialSettings =
        await OrganizationsService.readNotificationSettingsAsync(orgRoute);
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
      await OrganizationsService.updateNotificationSettingsAsync(
        orgRoute,
        settings
      );
      setIsChanged(false);
    } catch (err) {
      ErrorDlg.show(err, "Error Saving Notification Settings");
    }
    setIsSaving(false);
  };

  return (
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
                {WidgetStore.organizations.currentOrg?.details.name}{" "}
                Notification Settings
              </h1>
              <div className="mt-0 text-muted">
                Configure the messages sent to members of your organization
              </div>
            </Col>
            <Col md="1" className="text-right"></Col>
          </Row>
        </CardHeader>
        <div className="table-responsive">
          <ToggleTable title="Notification Channels">
            <ToggleRow
              title="E-Mail"
              subtitle="All users must have valid e-mails, so this will reach all users"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    channels: { ...settings.channels, isEmailEnabled: enabled },
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.channels?.isEmailEnabled ?? true}
            />
            <ToggleRow
              title="SMS"
              subtitle="Only sent to users with valid phone numbers in their profile"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    channels: { ...settings.channels, isSmsEnabled: enabled },
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.channels?.isSmsEnabled ?? true}
            />
          </ToggleTable>
        </div>
        <div className="table-responsive">
          <ToggleTable title="Notification Types">
            <ToggleRow
              title="Welcome Message"
              subtitle="When someone logs on for the first time"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({ ...settings, isWelcomeEnabled: enabled });
                setIsChanged(true);
              }}
              defaultValue={settings?.isWelcomeEnabled ?? true}
            />
            <ToggleRow
              title="Person Invite"
              subtitle="On manual invite; Never on directory sync"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    isPersonInviteEnabled: enabled,
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.isPersonInviteEnabled ?? true}
            />
            <ToggleRow
              title="Team Invite"
              subtitle="On manual invite to a team; Never on directory sync"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    isMemberInvitedEnabled: enabled,
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.isMemberInvitedEnabled ?? true}
            />
            <ToggleRow
              title="Session Reminder"
              subtitle="On request recordings from contributor"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    isSessionReminderEnabled: enabled,
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.isSessionReminderEnabled ?? true}
            />
            <ToggleRow
              title="Session Publish"
              subtitle="On request listen from audience member"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    isSessionPublishEnabled: enabled,
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.isSessionPublishEnabled ?? true}
            />
            <ToggleRow
              title="Session Publish for Host"
              subtitle="Send success message to host of session when published"
              onToggle={(enabled: boolean) => {
                settings != null &&
                  setSettings({
                    ...settings,
                    isSessionHostPublishEnabled: enabled,
                  });
                setIsChanged(true);
              }}
              defaultValue={settings?.isSessionHostPublishEnabled ?? true}
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
  );
});
