import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardFooter, CardHeader, Col, Row } from "reactstrap";

import {
  OrganizationsService,
  OrgSessionSettings,
  SessionSecurityType,
  SessionCommentPolicy,
} from "@soundbite/api";
import {
  ErrorDlg,
  Loader,
  OrganizationStore,
  WidgetStore,
} from "@soundbite/widgets-react";

import {
  DropDownRow,
  ToggleHeader,
  ToggleRow,
  ToggleTable,
} from "../controls/ToggleTable";

export const SessionSettingsCard: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  // State
  const [settings, setSettings] = useState<OrgSessionSettings | undefined>(
    undefined
  );
  const [isChanged, setIsChanged] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isTranscriptionEnabled, setTranscriptionEnabled] = useState(false);
  const [isSessionCommentsEnabled, setSessionCommentsEnabled] = useState(false);

  useEffect(() => {
    const load = async () => {
      const initialSettings =
        await OrganizationsService.readSessionSettingsAsync(orgRoute);
      setSettings(initialSettings);
      setTranscriptionEnabled(initialSettings.transcriptionEnabled);
      setSessionCommentsEnabled(initialSettings.sessionCommentsEnabled);
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
      let sessionSettings =
        await OrganizationsService.updateSessionSettingsAsync(
          orgRoute,
          settings
        );

      // Update the settings on the current org so they take effect immediately
      if (OrganizationStore.currentOrg) {
        OrganizationStore.currentOrg.settings.sessions = sessionSettings;
      }

      setIsChanged(false);
    } catch (err) {
      ErrorDlg.show(err, "Error Saving Session Settings");
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
                {WidgetStore.organizations.currentOrg?.details.name} Session
                Settings
              </h1>
              <div className="mt-0 text-muted">
                Configure the Soundbite session settings for your organization
              </div>
            </Col>
            <Col md="1" className="text-right"></Col>
          </Row>
        </CardHeader>
        <div className="table-responsive">
          <ToggleTable title="Security">
            <DropDownRow
              title="Default Security"
              subtitle="Specifies whether sessions should be protected or public by default"
              onChange={(value) => {
                if (settings != null) {
                  switch (value) {
                    case "Public":
                      setSettings({
                        ...settings,
                        defaultSessionSecurity: SessionSecurityType.Public,
                      });
                      break;
                    default:
                      setSettings({
                        ...settings,
                        defaultSessionSecurity: SessionSecurityType.Protected,
                      });
                      break;
                  }
                  setIsChanged(true);
                }
              }}
              defaultValue={
                settings?.defaultSessionSecurity === SessionSecurityType.Public
                  ? "Public"
                  : "Protected"
              }
              choices={[
                { value: "Protected", label: "Protected" },
                { value: "Public", label: "Public" },
              ]}
            />
            <ToggleHeader title={"Comments"} />
            <ToggleRow
              title="Enable Session Comments"
              subtitle="Determines whether comments are avialable for sessions"
              onToggle={(enabled: boolean) => {
                if (settings != null) {
                  setSettings({ ...settings, sessionCommentsEnabled: enabled });
                  setSessionCommentsEnabled(enabled);
                  setIsChanged(true);
                }
              }}
              defaultValue={settings?.sessionCommentsEnabled ?? true}
            />
            <ToggleRow
              disabled={!isSessionCommentsEnabled}
              title="Session Comments Enabled by Default"
              subtitle="Determines whether comments automatically enabled for sessions"
              onToggle={(enabled: boolean) => {
                if (settings != null) {
                  setSettings({
                    ...settings,
                    defaultSessionCommentPolicy: enabled
                      ? SessionCommentPolicy.Allowed
                      : SessionCommentPolicy.Disallowed,
                  });
                  setIsChanged(true);
                }
              }}
              defaultValue={
                (settings?.defaultSessionCommentPolicy ??
                  SessionCommentPolicy.Allowed) === SessionCommentPolicy.Allowed
                  ? true
                  : false
              }
            />
            <ToggleHeader title={"Transcription"} />
            <ToggleRow
              title="Enable Transcription"
              subtitle="Determines whether transcription is avialable for sessions"
              onToggle={(enabled: boolean) => {
                if (settings != null) {
                  setSettings({ ...settings, transcriptionEnabled: enabled });
                  setTranscriptionEnabled(enabled);
                  setIsChanged(true);
                }
              }}
              defaultValue={settings?.transcriptionEnabled ?? true}
            />
            <ToggleRow
              disabled={!isTranscriptionEnabled}
              title="Transcribe Sessions by Default"
              subtitle="Determines whether transcription is automatically enabled for sessions"
              onToggle={(enabled: boolean) => {
                if (settings != null) {
                  setSettings({
                    ...settings,
                    transcriptionOnByDefault: enabled,
                  });
                  setIsChanged(true);
                }
              }}
              defaultValue={settings?.transcriptionOnByDefault ?? true}
            />
            <ToggleRow
              title="Enable Wizard"
              subtitle="Determines whether Gen AI features based on the trascript are available for the sessions"
              disabled={true}
              onToggle={(enabled: boolean) => {
                // TODO: Implement this for real
                //if (settings != null) {
                //  setSettings({ ...settings, transcriptionEnabled: enabled });
                //  setTranscriptionEnabled(enabled);
                //  setIsChanged(true);
                //}
              }}
              defaultValue={false}
            />
            <ToggleHeader title={"Video"} />
            <ToggleRow
              title="Enable Video"
              subtitle="Determines whether people can create video content in sessions"
              onToggle={(enabled: boolean) => {
                if (settings != null) {
                  setSettings({ ...settings, videoEnabled: enabled });
                  setIsChanged(true);
                }
              }}
              defaultValue={settings?.videoEnabled ?? true}
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
