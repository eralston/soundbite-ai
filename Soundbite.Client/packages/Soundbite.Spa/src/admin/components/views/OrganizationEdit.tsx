import React, { Fragment, useEffect, useState } from "react";
import { observer } from "mobx-react-lite";
import {
  Card,
  Row,
  CardHeader,
  Col,
  FormGroup,
  InputGroup,
  CardBody,
  Button,
} from "reactstrap";
import { useHistory, useParams } from "react-router-dom";

import AdminStore from "../../store/AdminStore";
import Layout from "../Layout";
import { OrganizationExtended } from "@soundbite/api";
import { Loader, ShowWhen } from "@soundbite/widgets-react";

import { PatchService } from "../../../modules/PatchService";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////

interface IProps {}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

const Component: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  // Parameters and variables
  let { orgRoute } = useParams<{ orgRoute: string }>();
  let isNew = orgRoute?.toLowerCase() === "create";
  const history = useHistory();

  //////////[ State Management ]////////////////////////////////////////////////////////////////////

  // UI Data
  let [existingItem, setExistingItem] = useState<OrganizationExtended | null>(
    null
  );
  let [loading, setLoading] = useState(true);
  let [notFound, setNotFound] = useState(false);
  let [disabled, setDisabled] = useState(false);
  let [title, setTitle] = useState("");

  // Form Data
  let [name, setName] = useState("");
  let [description, setDescription] = useState("");
  let [orgImage, setOrgImage] = useState<any>(null);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // TODO: Determine if it's possible to refactor initialize, since this should probably happen in a callback per eslint
  /* eslint-disable */
  async function initialize() {
    if (orgRoute && orgRoute.toLowerCase() === "create") {
      // Creating a new item
      setLoading(false);
    } else {
      // Editing an existing item
      try {
        let item = await AdminStore.orgStore.readOrgByRoute(orgRoute);
        if (item) {
          setExistingItem(item);
          setTitle(item.name);
          setName(item.name);
          setDescription(item.description);
          setNotFound(false);
        } else {
          setNotFound(true);
        }
      } finally {
        setLoading(false);
      }
    }
  }
  /* eslint-enable */

  // Calling useEffect with [] as the second parameter ensures one-time execution
  useEffect(() => {
    initialize();
  }, [initialize]);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onCancel() {
    history.goBack();
  }

  async function onSave() {
    try {
      // Disable the form
      setDisabled(true);
      let route = existingItem?.route ?? null;

      // Determine how to process the form
      if (isNew) {
        //await AdminStore.orgStore.createOrg()
        const updated = await AdminStore.orgStore.createOrg({
          name,
          description,
        } as OrganizationExtended);
        route = updated?.route;
      } else if (existingItem) {
        var patchService = new PatchService();
        var patch = patchService.generatePatch(existingItem, {
          name,
          description,
        });
        if (patch && patch.length > 0) {
          await AdminStore.orgStore.updateOrg(existingItem.route, patch);
        }
      }

      // Determine whether an org image was uploaded and if so proces it
      if (orgImage && route) {
        await AdminStore.orgStore.uploadOrgImage(
          route,
          orgImage,
          orgImage.name
        );
      }

      history.goBack();
    } finally {
      setDisabled(false);
    }
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Layout>
      <Card>
        <CardHeader className="bg-transparent">
          <Row className="align-items-center">
            <Col>
              <h3 className="mb-0">
                {isNew ? "Create" : "Edit"} Organization {title}
              </h3>
            </Col>
          </Row>
        </CardHeader>
        <CardBody>
          <Loader isLoadedWhen={!loading}>
            <ShowWhen is={notFound}>
              Organization with route '{orgRoute}' was not found.
            </ShowWhen>
            <ShowWhen is={!notFound}>
              <FormGroup>
                <InputGroup>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Organization Name"
                    aria-label="Organization Name"
                    defaultValue={name}
                    onChange={(e) => setName(e.target.value)}
                    disabled={disabled}
                    title="Organization Name"
                  />
                </InputGroup>
              </FormGroup>
              <FormGroup>
                <textarea
                  className="form-control"
                  placeholder="Organization Description"
                  defaultValue={description}
                  onChange={(e) => setDescription(e.target.value)}
                  disabled={disabled}
                  title="Organization Description"
                ></textarea>
              </FormGroup>
              <FormGroup>
                <input
                  type="File"
                  className="btn-icon btn-3 btn-block"
                  color="secondary"
                  onChange={(e: any) => setOrgImage(e.target.files[0])}
                  disabled={disabled}
                />
              </FormGroup>

              <Row>
                <Col>
                  <Button
                    className="btn-icon btn-3 btn-block"
                    type="button"
                    color="secondary"
                    onClick={onCancel}
                    disabled={disabled}
                    title="Cancel"
                  >
                    <span className="btn-inner--icon">
                      <i className="fas fa-times-circle"></i>
                    </span>
                    <span className="btn-inner--text">
                      <Fragment>&nbsp;</Fragment>Cancel
                    </span>
                  </Button>
                </Col>
                <Col>
                  <Button
                    className="btn-icon btn-3 btn-block"
                    type="button"
                    color="primary"
                    onClick={onSave}
                    disabled={disabled}
                    title="Save Organization"
                  >
                    <span className="btn-inner--icon">
                      <i className="fas fa-save "></i>
                    </span>
                    <span className="btn-inner--text">
                      <Fragment>&nbsp;</Fragment>Save
                    </span>
                  </Button>
                </Col>
              </Row>
            </ShowWhen>
          </Loader>
        </CardBody>
      </Card>
    </Layout>
  );
});

export default Component;
